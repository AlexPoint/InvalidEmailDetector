using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpEntropy;

namespace InvalidEmailDetector.src
{
    public class MaximumEntropyInvalidEmailDetector
    {
        // Properties ---------------

        /// <summary>
        /// The maximum entropy model to use to evaluate contexts.
        /// </summary>
        private readonly IMaximumEntropyModel _model;

        /// <summary>
        /// The feature context generator.
        /// </summary>
        private readonly IContextGenerator<string> _contextGenerator;
        

        // Constructors ------------------

        /// <summary>
        /// Constructor which takes a IMaximumEntropyModel and calls the three-arg
        /// constructor with that model, a SentenceDetectionContextGenerator, and the
        /// default end of sentence scanner.
        /// </summary>
        /// <param name="model">
        /// The MaxentModel which this SentenceDetectorME will use to
        /// evaluate end-of-sentence decisions.
        /// </param>
        public MaximumEntropyInvalidEmailDetector(IMaximumEntropyModel model)
        {
            _contextGenerator = new InvalidEmailDetectionContextGenerator();
            _model = model;
        }

        public MaximumEntropyInvalidEmailDetector(string name)
            : this(new GisModel(new SharpEntropy.IO.BinaryGisModelReader(name))) { }

        
        // Methods ----------------

        /// <summary> 
        /// Tests the probabilty of an email to be invalid.
        /// </summary>
        /// <param name="email">
        /// The email to be processed.
        /// </param>
        /// <returns>   
        /// A string array containing individual sentences as elements.
        /// </returns>
        public double GetInvalidProbability(string email)
        {
            try
            {
                var context = _contextGenerator.GetContext(email);
                double[] probabilities = _model.Evaluate(context);
                return probabilities.Last();
            }
            catch (KeyNotFoundException ex)
            {
                // This case should not happen if we trained the model with enough data
                return 0.5;
            }
        }


        // Utilities ----------------------------

        /// <summary>
        /// Trains a model with an input file with the following format:
        /// jane.doe@gmail.com  0
        /// lqsmkdqmsdkl@qsdkqsmdk  1
        /// ...
        /// The first line represents a valid email, the second an invalid.
        /// </summary>
        /// <param name="inputFilePath">The path of the input file</param>
        /// <param name="iterations">The number of iterations for the training</param>
        /// <param name="cut">The cut for the training</param>
        /// <returns>The trained GisModel</returns>
        public static GisModel TrainModel(string inputFilePath, int iterations, int cut)
        {
            return TrainModel(new List<string>() { inputFilePath }, iterations, cut);
        }

        /// <summary>
        /// Trains a model with a collection of input files with the following format:
        /// jane.doe@gmail.com  0
        /// mqsldkqsmlqsmdklqs@sdlsqjd  1
        /// ...
        /// The first line represents a valid email, the second an invalid.
        /// </summary>
        /// <param name="filePaths">The collection of file paths</param>
        /// <param name="iterations">The number of iterations for the training</param>
        /// <param name="cut">The cut for the training</param>
        /// <returns>The trained GisModel</returns>
        public static GisModel TrainModel(IEnumerable<string> filePaths, int iterations, int cut)
        {
            var trainer = new GisTrainer();

            foreach (var file in filePaths)
            {
                using (var streamReader = new StreamReader(file))
                {
                    ITrainingDataReader<string> dataReader = new PlainTextByLineDataReader(streamReader);
                    ITrainingEventReader eventReader = new InvalidEmailDetectionEventReader(dataReader);

                    trainer.TrainModel(eventReader, iterations, cut);
                }
            }

            return new GisModel(trainer);
        }

        /// <summary>
        /// Trains a model with a collection of emails and a flag to indicate invalidity.
        /// </summary>
        /// <param name="emailsAndValidities">A collection of emails and whether this email is invalid</param>
        /// <param name="iterations">The number of iterations for the training</param>
        /// <param name="cut">The cut for the training</param>
        /// <returns>The trained GisModel</returns>
        public static GisModel TrainModel(IEnumerable<EmailAndValidity> emailsAndValidities, int iterations, int cut)
        {
            var trainer = new GisTrainer();
            var eventReader = new InvalidEmailDetectionDataEventReader(emailsAndValidities);
            trainer.TrainModel(eventReader, iterations, cut);

            return new GisModel(trainer);
        }
    }
}
