InvalidEmailDetector
====================

A machine-learning approach to detect invalid emails without actually sending an email.

Whether you use an external service for email sending (Amazon SES, Mailchimp...) or you do it yourself, you have to watch your bounce rate as your reputation highly depend on it.  
Amazon SES recommends a 5% bounce rate for example.  
Problem is: there is no way to know if an email address is valid... unless you send an email and watch for the bounce.  
So if a large portion of your registered users use invalid email addresses, you are stuck!

This project aims at solving, at least partially, this issue.  
A machine learning algorithm computes the probability of an email address to be valid given your historical data. This way, you can prioritize the recipients of your emails, being able to validate more users' emails (and send them additional emails later if need be).

Two main operations are necessary:
- the training of your model with historical data (through a well-formatted file or specific method)

```csharp
var iterations = 100;
var cut = 5;
// train the model from a data set
var emailsAndValidities = new List<EmailAndValidity>()
{
    new EmailAndValidity() {Email = "john.doe@gmail.com", IsInvalid = false},
    new EmailAndValidity() {Email = "sqmldklsqmdkxmkqds@qsldkjsqd.qsld", IsInvalid = true},
    // ...
};

var model = MaximumEntropyInvalidEmailDetector.TrainModel(emailsAndValidities, iterations, cut);

// train the model from a formatted file
var inputFilePath = currentDirectory + "Input/invalidEmailDetection.train";
var model2 = MaximumEntropyInvalidEmailDetector.TrainModel(inputFilePath, iterations, cut);
```
- the use of the detector
```csharp
// use trained model to build a detector
var invalidEmailDetector = new MaximumEntropyInvalidEmailDetector(model);

// use the detector to compute the probability of invalidity of new email addresses
var proba1 = invalidEmailDetector.GetInvalidProbability("john.doe@yopmail.com");
var proba2 = invalidEmailDetector.GetInvalidProbability("qslkdjqlsdj@qsjdh.qds");
```
