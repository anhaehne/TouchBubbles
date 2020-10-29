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
        public Profile(string name, 
            IObservable<IReadOnlyCollection<Entity>> allEntities, 
            Guid? profileId = null,
            IEnumerable<string>? entityIds = null)
        {
            Name = name;
            Id = profileId ?? Guid.NewGuid();
            EntityIds = new RangeObservableCollection<string>();
            Entities = EntityIds
                .ToCollectionObservable()
                .Zip(allEntities,
                (ids, entities) => ids
                    .Select(id => entities.SingleOrDefault(e => e.Id == id))
                    .Where(x => x is not null)
                    .Cast<Entity>()
                    .ToList());

            EntityIds.AddRange(entityIds);
        }

        private Profile()
        {
            Name = string.Empty;
            EntityIds = new RangeObservableCollection<string>();
            Entities = Observable.Empty<IReadOnlyCollection<Entity>>();
        }

        public string Name { get; set; }

        public Guid Id { get; }

        public RangeObservableCollection<string> EntityIds { get; }

        public IObservable<IReadOnlyCollection<Entity>> Entities { get; }

        public static Profile Empty { get; } = new Profile();

        public static ProfileDto ToDto(Profile profile)
        {
            return new ProfileDto {Id = profile.Id, EntityIds = profile.EntityIds, Name = profile.Name};
        }
    }
}