using CommunityToolkit.Mvvm.Messaging.Messages;
using ConcertJournal.Models;

namespace ConcertJournal.Messages
{
    // A simple message that carries the concert involved in the change
    public class ConcertListChangedMessage : ValueChangedMessage<Concert>
    {
        public ConcertListChangedMessage(Concert value) : base(value)
        {
        }
    }
}