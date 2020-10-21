using System;
using System.Collections.ObjectModel;
using System.Linq;
using TouchBubbles.Shared.Models.HomeAssistant;

namespace TouchBubbles.Shared.Models
{
    public class Profile
    {
        private string _name;

        public Profile(string name, Guid? id = null, ObservableCollection<Entity>? entities = null)
        {
            _name = name;
            Id = id ?? Guid.NewGuid();
            Entities = entities ?? new ObservableCollection<Entity>();
            Entities.CollectionChanged += (s, e) => Changed?.Invoke(this, this);
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                Changed?.Invoke(this, this);
            }
        }

        public Guid Id { get; }

        public ObservableCollection<Entity> Entities { get; }

        public event EventHandler<Profile>? Changed;

        public static ProfileDto ToDto(Profile profile) => new ProfileDto { Id = profile.Id, EntityIds = profile.Entities.Select(x => x.Id).ToList(), Name = profile.Name };
    }
}