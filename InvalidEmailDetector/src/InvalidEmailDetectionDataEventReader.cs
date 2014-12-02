using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpEntropy;

namespace InvalidEmailDetector.src
{
    public class InvalidEmailDetectionDataEventReader: ITrainingEventReader
    {
        private readonly List<EmailAndValidity> _emailsAndValidities;
        private int _currentIndex;
        private readonly IContextGenerator<string> _contextGenerator;

        public InvalidEmailDetectionDataEventReader(IEnumerable<EmailAndValidity> emailsAndValidities)
        {
            this._currentIndex = 0;
            this._emailsAndValidities = emailsAndValidities.ToList();
            this._contextGenerator = new InvalidEmailDetectionContextGenerator();
        }

        public TrainingEvent ReadNextEvent()
        {
            // read current email/flag
            var emailAndValidity = _emailsAndValidities[_currentIndex];
            var type = emailAndValidity.IsInvalid ? "INV" : "OK";
            var email = emailAndValidity.Email;
            // create event
            var nextEvent = new TrainingEvent(type, _contextGenerator.GetContext(email));
            // increase current index
            _currentIndex++;

            return nextEvent;
        }

        public bool HasNext()
        {
            return _currentIndex < _emailsAndValidities.Count();
        }
    }
}
