using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConfReboot.Specs
{
    [TestClass]
    public class Manage_conference
    {
        // As a conference organiser
        // I want to be able to manage a conference
        // In order to have a proper organisation

        [TestMethod]
        public void Register_a_conference()
        {
            var Sut = new SUTClass();
            var confdate = DateTime.Today;
            Sut.RegisterType<Conference>();

            Sut.When(x => x.RegisterConference(ConferenceId: "conferences/1", Name: "ExampleConf", StartDate: confdate, MaxSeats: 100));
            Sut.Then(x => x.ConferenceRegistered(ConferenceId: "conferences/1", Name: "ExampleConf", StartDate: confdate, MaxSeats: 100));
        }
    }
}