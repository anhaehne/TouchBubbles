using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using TouchBubbles.Shared.Models.HomeAssistant;

namespace TouchBubbles.Shared.Models
{
    public class Profile
    {
        private string _name;

        public Profile(string name, 
            IObservable<IReadOnlyCollection<Entity>> allEntities, 
            Guid? profileId = null,
            IEnumerable<string>? entityIds = null)
        {
            _name = name;
            Id = profileId ?? Guid.NewGuid();
            EntityIds = new RangeObservableCollection<string>(entityIds ?? Enumerable.Empty<string>());
            Entities = EntityIds
                .ToCollectionObservable()
                .CombineLatest(allEntities)
                .Select(e => e.First.Select(id => e.Second.SingleOrDefault(e => e.Id == id))
                    .Where(x => x is not null)
                    .Cast<Entity>()
                    .ToList());

            EntityIds.CollectionChanged += (sender, args) => Changed?.Invoke(this, this);
        }

        private Profile()
        {
            _name = string.Empty;
            EntityIds = new RangeObservableCollection<string>();
            Entities = Observable.Empty<IReadOnlyCollection<Entity>>();
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

        public RangeObservableCollection<string> EntityIds { get; }

        public IObservable<IReadOnlyCollection<Entity>> Entities { get; }

        public static Profile Empty { get; } = new Profile();

        public event EventHandler<Profile>? Changed; 

        public static ProfileDto ToDto(Profile profile)
        {
            return new ProfileDto {Id = profile.Id, EntityIds = profile.EntityIds, Name = profile.Name};
        }
    }
}