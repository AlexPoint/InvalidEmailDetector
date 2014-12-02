using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvalidEmailDetector.src;

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
    }
}
