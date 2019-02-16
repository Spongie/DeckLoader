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
using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Models;
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
        private string currentCard;
        private string deckName;
        private string selectedImagePath;
        private int currentCombinedProgress;
        private bool canStartJob;
        private string log;
        const string apiFileName = "scryfall-default-cards.json";

        public ViewModel()
        {
            CurrentDownloadProgress = 0;
            CurrentCombinedProgress = 0;
            CardCount = 100;
            canStartJob = true;

            OutputFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "Tabletop Simulator", "Saves", "Saved Objects");

            if (!Directory.Exists("Cards"))
            {
                Directory.CreateDirectory("Cards");
            }
        }

        public string DeckName
        {
            get { return deckName; }
            set
            {
                deckName = value;
                FirePropertChanged();
            }
        }

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

        public int CurrentCombinedProgress
        {
            get { return currentCombinedProgress; }
            set
            {
                currentCombinedProgress = value;
                FirePropertChanged();
            }
        }

        public bool CarStartJob
        {
            get { return canStartJob; }
            set
            {
                canStartJob = value;
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
            try
            {
                CarStartJob = false;
                Log = "Loading API file..." + Environment.NewLine;

                var lines = File.ReadAllLines(selectedFile);
                CardCount = lines.Length;
                var httpClient = new HttpClient();
                var saveFileInfo = new Dictionary<string, string>();

                List<Card> cards = JsonConvert.DeserializeObject<List<Card>>(File.ReadAllText(apiFileName));

                DownloadCardImages(lines, saveFileInfo, cards);

                var tableTopObject = new TableTopObject();
                var deck = tableTopObject.ObjectStates.First();

                CombineImages(saveFileInfo, deck);
                UploadImages(tableTopObject, deck);

                Log += "Done!";
            }
            catch (Exception e)
            {
                string message = "Error on Card: " + currentCard + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace;

                if (e.InnerException != null)
                {
                    message += Environment.NewLine + Environment.NewLine + e.InnerException.Message;
                }

                System.Windows.Forms.MessageBox.Show(message);
                Log += "Interruppted because of error...";
            }
            finally
            {
                CarStartJob = true;
            }
        }

        private void DownloadCardImages(string[] lines, Dictionary<string, string> saveFileInfo, List<Card> cards)
        {
            Log += "Downloading images..." + Environment.NewLine;
            foreach (var line in lines)
            {
                currentCard = line;

                if (!int.TryParse(line.Split(' ').First(), out int count))
                {
                    continue;
                }
                
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
        }

        private void CombineImages(Dictionary<string, string> saveFileInfo, TableTopDeck deck)
        {
            const int cardWidth = 409;
            const int cardHeight = 585;
            int fileIndex = 0;
            var keys = saveFileInfo.Keys.ToArray();
            var files = Directory.GetFiles("Cards");
            int baseId = 100;

            Log += "Combining images..." + Environment.NewLine;

            for (int i = 0; i < 100; i++)
            {
                if (fileIndex >= keys.Length)
                {
                    break;
                }

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

                ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                var myEncoder = Encoder.Quality;
                var myEncoderParameters = new EncoderParameters(1);

                var myEncoderParameter = new EncoderParameter(myEncoder, 90L);
                myEncoderParameters.Param[0] = myEncoderParameter;

                bitmap.Save(outputFolder + "\\" + DeckName + "_Part_" + (i + 1).ToString() + ".jpg", jpgEncoder, myEncoderParameters);
                baseId += 100;
            }
        }

        private void UploadImages(TableTopObject tableTopObject, TableTopDeck deck)
        {
            Log += "Uploading image 1/2..." + Environment.NewLine;
            UploadAndSetImageLink(outputFolder + "\\" + DeckName + "_Part_1.jpg", deck, 1);

            Log += "Uploading image 2/2..." + Environment.NewLine;
            UploadAndSetImageLink(outputFolder + "\\" + DeckName + "_Part_2.jpg", deck, 2);

            Log += "Saving deck to output..." + Environment.NewLine;
            File.WriteAllText(outputFolder + "\\" + DeckName + ".json", JsonConvert.SerializeObject(tableTopObject, Formatting.Indented, new TableTopCustomDeckJson()));
            File.Copy(SelectedImagePath, outputFolder + "\\" + DeckName + new FileInfo(SelectedImagePath).Extension);
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
            var client = new ImgurClient("b99e60a50806ebe", "e5483212a3c81bae011d6e11deb62fdcf4d2d092");
            var endpoint = new ImageEndpoint(client);

            using (var fs = new FileStream(imagePath, FileMode.Open))
            {
                IImage image= endpoint.UploadImageStreamAsync(fs).Result;

                deck.CustomDeck.TableTopDeckInfos.Add(index, new TableTopDeckImageInfo
                {
                    FaceURL = image.Link
                });

                foreach (var containedObject in deck.ContainedObjects)
                {
                    int key = containedObject.CustomDeck.TableTopDeckInfos.Keys.First();

                    if (key == index)
                    {
                        containedObject.CustomDeck.TableTopDeckInfos[key].FaceURL = image.Link;
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
