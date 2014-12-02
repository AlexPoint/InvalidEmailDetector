using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvalidEmailDetector.src;
using SharpEntropy;

namespace Samples
{
    class Program
    {
        private static readonly string currentDirectory = Environment.CurrentDirectory + "/../../";

        static void Main(string[] args)
        {
            // Training -------------------------------------
            // train the model from a data set
            var emailsAndValidities = new List<EmailAndValidity>()
            {
                new EmailAndValidity() {Email = "john.doe@gmail.com", IsInvalid = false},
                new EmailAndValidity() {Email = "jane.doe@gmail.com", IsInvalid = false},
                new EmailAndValidity() {Email = "john.doe@hotmail.fr", IsInvalid = false},
                new EmailAndValidity() {Email = "jane.doe@hotmail.com", IsInvalid = false},
                new EmailAndValidity() {Email = "mkqljsdmqlskfmldwx@gmail.com", IsInvalid = true},
                new EmailAndValidity() {Email = "sqmldklsqmdkxmkqds@qsldkjsqd.qsld", IsInvalid = true},
                new EmailAndValidity() {Email = "thisisnotavalidemail@qmsldjk.qsd", IsInvalid = true},
                new EmailAndValidity() {Email = "qs@qq.ss", IsInvalid = true},
            };

            var iterations = 100;
            var cut = 5;
            var model = MaximumEntropyInvalidEmailDetector.TrainModel(emailsAndValidities, iterations, cut);

            // train the model from a formatted file
            var inputFilePath = currentDirectory + "Input/invalidEmailDetection.train";
            var model2 = MaximumEntropyInvalidEmailDetector.TrainModel(inputFilePath, iterations, cut);

            // Detection -----------------------------------
            // use trained model to build a detector
            var invalidEmailDetector = new MaximumEntropyInvalidEmailDetector(model2);

            // use the detector to compute the probability of invalidity of new email addresses
            var newEmailAddresses = new List<string>() {"john.doe@yopmail.com", "tt@s.s", "qmsdlkqsmqlmsdkq@sqs.qsd"};
            foreach (var newEmailAddress in newEmailAddresses)
            {
                var probabilityOfInvalidity = invalidEmailDetector.GetInvalidProbability(newEmailAddress);
                Console.WriteLine("{0} -> proba invalidity: {1}", newEmailAddress, probabilityOfInvalidity);
            }

            Console.WriteLine("----");
            Console.ReadLine();
        }

        static void Statistics(GisModel model, string testFilePath)
        {
            var results = new List<Tuple<string, bool, double>>();

            // create detector from model
            var invalidEmailDetector = new MaximumEntropyInvalidEmailDetector(model);

            // read all test (email + invalid flag) lines
            var allLines = File.ReadAllLines(testFilePath);
            foreach (var line in allLines)
            {
                var parts = line.Split('\t');
                var email = parts.First();
                var isInvalid = parts.Last() == "1";
                
                // store the result of the detection as well as the actual validity of the email
                var invalidProbability = invalidEmailDetector.GetInvalidProbability(email);
                results.Add(new Tuple<string, bool, double>(email, isInvalid, invalidProbability));
            }

            // pretend to send the emails, and stop when we sent to many emails that bounced
            var nbOfEmailsSent = 0;
            var nbOfEmailsSentWhichWouldBounce = 0;
            var maxNbOfBouncePerDay = 25;
            foreach (var result in results.OrderBy(tup => tup.Item3))
            {
                if (nbOfEmailsSentWhichWouldBounce < maxNbOfBouncePerDay)
                {
                    nbOfEmailsSent++;
                    if (result.Item2) { nbOfEmailsSentWhichWouldBounce++; }
                }
                Console.WriteLine("{0} ({1})", result.Item1, result.Item2 ? "INVALID": "OK");
            }

            Console.WriteLine("Email that could have been sent: {0}", nbOfEmailsSent);
            Console.WriteLine("========");

            // Quick summary of the detection
            var probaOfInvalidity = 0.2497;
            var nbOfSamples = results.Count;
            Console.WriteLine("Nb of samples: {0}", nbOfSamples);
            var nbOfCorrectResults = results.Count(tup => tup.Item2 == tup.Item3 > probaOfInvalidity);
            Console.WriteLine("Nb of correct results: {0}", nbOfCorrectResults);
            var nbOfNotDetected = results.Count(tup => tup.Item2 && !(tup.Item3 > probaOfInvalidity));
            Console.WriteLine("Nb of not detected: {0}", nbOfNotDetected);
            var nbOfFalsePositive = results.Count(tup => !tup.Item2 && (tup.Item3 > probaOfInvalidity));
            Console.WriteLine("Nb of false positive: {0}", nbOfFalsePositive);
        }
    }
}
