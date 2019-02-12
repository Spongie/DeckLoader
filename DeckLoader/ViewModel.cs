using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading;
using DeckLoader.Models;
using DeckLoader.Models.TableTop;
using Newtonsoft.Json;

namespace DeckLoader
{
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string outputFolder;
        private string selectedFile;
        private int cardCount;
        private int currentDownloadProgress;
        private Thread workerThread;
        const string apiFileName = "scryfall-default-cards.json";

        public ViewModel()
        {
            CurrentDownloadProgress = 0;
            CurrentCombinedProgress = 0;
            CardCount = 100;

            OutputFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "Tabletop Simulator", "Saves", "Saved Objects");

            if (!Directory.Exists("Cards"))
            {
                Directory.CreateDirectory("Cards");
            }
        }

        private string deckName;

        public string DeckName
        {
            get { return deckName; }
            set
            {
                deckName = value;
                FirePropertChanged();
            }
        }

        private string log;

        public string Log
        {
            get { return log; }
            set
            {
                log = value;
                FirePropertChanged();
            }
        }


        public string OutputFolder
        {
            get { return outputFolder; }
            set
            {
                outputFolder = value;
                FirePropertChanged();
            }
        }

        private string selectedImagePath;

        public string SelectedImagePath
        {
            get { return selectedImagePath; }
            set
            {
                selectedImagePath = value;
                FirePropertChanged();
            }
        }


        public string SelectedFile
        {
            get { return selectedFile; }
            set
            {
                selectedFile = value;
                FirePropertChanged();
            }
        }

        public int CurrentDownloadProgress
        {
            get { return currentDownloadProgress; }
            set
            {
                currentDownloadProgress = value;
                FirePropertChanged();
            }
        }

        private int currentCombinedProgress;

        public int CurrentCombinedProgress
        {
            get { return currentCombinedProgress; }
            set
            {
                currentCombinedProgress = value;
                FirePropertChanged();
            }
        }


        public int CardCount
        {
            get { return cardCount; }
            set
            {
                cardCount = value;
                FirePropertChanged();
            }
        }

        public void StartDownload()
        {
            CurrentDownloadProgress = 0;
            CurrentCombinedProgress = 0;
            workerThread = new Thread(DownloadImages);
            workerThread.Start();
        }

        public void DownloadImages()
        {
            Log = "Loading API file..." + Environment.NewLine;

            var lines = File.ReadAllLines(selectedFile);
            CardCount = lines.Length;
            var httpClient = new HttpClient();
            var saveFileInfo = new Dictionary<string, string>();

            List<Card> cards = JsonConvert.DeserializeObject<List<Card>>(File.ReadAllText(apiFileName));

            Log += "Downloading images..." + Environment.NewLine;
            foreach (var line in lines)
            { 
                int count = int.Parse(line.Split(' ').First());
                string cardName = line.Substring(line.IndexOf(' ') + 1).Trim().Replace("/", " // ");

                var imageLinks = GetCardImages(cardName, cards).ToArray();

                for (int imageIndex = 0; imageIndex < imageLinks.Length; imageIndex++)
                {
                    var imageLink = imageLinks[imageIndex];

                    using (var client = new WebClient())
                    {
                        var file = "Cards\\" + cardName.Replace(" ", string.Empty).Replace("'", string.Empty).Replace("//", "_").Replace(",", string.Empty) + (imageIndex > 0 ? "_Back" : "") + ".jpg";

                        if (!File.Exists(file))
                        {
                            client.DownloadFile(imageLink, file);
                        }

                        if (saveFileInfo.Values.Any(v => v == cardName))
                        {
                            saveFileInfo.Add(file, cardName + "_Back");
                        }
                        else
                        {
                            saveFileInfo.Add(file, cardName);
                        }

                        for (int i = 1; i < count; i++)
                        {
                            string copyFileName = "Cards\\" + cardName.Replace(" ", string.Empty).Replace("'", string.Empty).Replace("//", "_").Replace(",", string.Empty) + i.ToString() + ".jpg";

                            if (!File.Exists(copyFileName))
                            {
                                File.Copy(file, copyFileName);
                            }
                            saveFileInfo.Add(copyFileName, cardName);
                        }
                    }
                }

                Interlocked.Increment(ref currentDownloadProgress);
                FirePropertChanged(nameof(CurrentDownloadProgress));
            }

            var tableTopObject = new TableTopObject();
            var deck = tableTopObject.ObjectStates.First();

            const int cardWidth = 409;
            const int cardHeight = 585;
            int fileIndex = 0;
            var keys = saveFileInfo.Keys.ToArray();
            var files = Directory.GetFiles("Cards");
            int baseId = 100;

            Log += "Combining images..." + Environment.NewLine;

            for (int i = 0; i < 2; i++)
            {
                int currentId = 0;
                var bitmap = new Bitmap(4096, 4096);
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    for (int y = 0; y < 7; y++)
                    {
                        if (fileIndex >= keys.Length)
                        {
                            break;
                        }

                        for (int x = 0; x < 10; x++)
                        {
                            if (fileIndex >= keys.Length)
                            {
                                break;
                            }

                            string file = keys[fileIndex];
                            using (var image = Image.FromFile(file))
                            {
                                graphics.DrawImage(image, x * cardWidth, y * cardHeight, cardWidth, cardHeight);
                            }

                            CurrentCombinedProgress++;

                            deck.DeckIDs.Add(baseId + currentId);
                            deck.ContainedObjects.Add(new TableTopCard
                            {
                                CardID = baseId + currentId,
                                Nickname = saveFileInfo[file],
                            });

                            deck.ContainedObjects.Last().CustomDeck.TableTopDeckInfos.Add(i + 1, new TableTopDeckImageInfo
                            {
                                FaceURL = (i + 1).ToString()
                            });

                            fileIndex++;
                            currentId++;

                            if (fileIndex >= files.Length)
                            {
                                break;
                            }
                        }
                    }
                }

                bitmap.Save(outputFolder + "\\" + DeckName + "_Part_" + (i + 1).ToString() + ".png");
                baseId += 100;
            }

            Log += "Uploading image 1/2..." + Environment.NewLine;
            UploadAndSetImageLink(outputFolder + "\\" + DeckName + "_Part_1.png", deck, 1);

            Log += "Uploading image 2/2..." + Environment.NewLine;
            UploadAndSetImageLink(outputFolder + "\\" + DeckName + "_Part_2.png", deck, 2);

            Log += "Saving deck to output..." + Environment.NewLine;
            File.WriteAllText(outputFolder + "\\" + DeckName + ".json", JsonConvert.SerializeObject(tableTopObject, Formatting.Indented, new TableTopCustomDeckJson()));
            File.Copy(SelectedImagePath, outputFolder + "\\" + DeckName + new FileInfo(SelectedImagePath).Extension); 

            Log += "Done!";
        }

        private IEnumerable<string> GetCardImages(string cardName, List<Card> cards)
        {
            var foundCard = cards.Where(card =>
                card.Name == cardName
                && !card.Promo
                && card.Nonfoil
                && card.Image_uris != null
                && !string.IsNullOrEmpty(card.Image_uris.Png))
                .LastOrDefault();

            if (foundCard == null)
            {
                foundCard = cards.Where(card =>
                    card.Name.StartsWith(cardName)
                    && !card.Promo
                    && card.Nonfoil)
                    .LastOrDefault();

                return foundCard.CardFaces.Select(face => face.Image_uris.Png);
            }

            return new[] { foundCard.Image_uris.Png };
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
        
        private void UploadAndSetImageLink(string imagePath, TableTopDeck deck, int index)
        {
            using (var httpClient = new HttpClient())
            {
                var form = new MultipartFormDataContent();

                var bytes = File.ReadAllBytes(imagePath);
                var imageForm = new ByteArrayContent(bytes, 0, bytes.Count());
                imageForm.Headers.ContentType = new MediaTypeHeaderValue("image/jpg");

                form.Add(imageForm, "image", "nameholder.png");

                HttpResponseMessage response = httpClient.PostAsync("https://api.put.re/upload", form).Result;

                response.EnsureSuccessStatusCode();
                string imageLink = JsonConvert.DeserializeObject<UploadResponse>(response.Content.ReadAsStringAsync().Result).Data.Link;

                foreach (var containedObject in deck.ContainedObjects)
                {
                    int key = containedObject.CustomDeck.TableTopDeckInfos.Keys.First();

                    if (key == index)
                    {
                        containedObject.CustomDeck.TableTopDeckInfos[key].FaceURL = imageLink;
                    }
                }
            }
                
        }

        private void FirePropertChanged([CallerMemberName]string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
