using MetaParser.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace MetaParser.Tests
{
    [TestClass]
    public class RuleTests
    {
        [TestMethod]
        public void Rule_StateChanged_RaisesStateChangedEvent()
        {
            var receivedEvents = new List<StateChangedEventArgs>();
            var rule = new Rule()
            {
                State = "asdf"
            };

            rule.StateChanged += (s, args) => receivedEvents.Add(args);

            rule.State = "fdsa";

            Assert.AreEqual(1, receivedEvents.Count);
            Assert.AreEqual("asdf", receivedEvents[0].OldState);
            Assert.AreEqual("fdsa", receivedEvents[0].NewState);
        }
    }
}
