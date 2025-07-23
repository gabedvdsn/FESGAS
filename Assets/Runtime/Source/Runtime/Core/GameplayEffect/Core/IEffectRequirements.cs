using System.Collections.Generic;
using System.Net.Security;

namespace FESGameplayAbilitySystem
{
    public interface IEffectRequirements
    {
        public bool CheckApplicationRequirements(List<ITag> tags);
        public bool CheckRemovalRequirements(List<ITag> tags);
        public bool CheckOngoingRequirements(List<ITag> tags);

        public static EmptyEffectRequirements GenerateEmptyRequirements()
        {
            return new EmptyEffectRequirements();
        }

        public static EmptyEffectRequirements Generate(AvoidRequireTagGroup application, AvoidRequireTagGroup removal, AvoidRequireTagGroup ongoing)
        {
            return new EmptyEffectRequirements(application, removal, ongoing);
        }
    }

    public class EmptyEffectRequirements : IEffectRequirements
    {
        private AvoidRequireTagGroup Application;
        private AvoidRequireTagGroup Removal;
        private AvoidRequireTagGroup Ongoing;

        public EmptyEffectRequirements()
        {
            Application = AvoidRequireTagGroup.GenerateEmpty();
            Removal = AvoidRequireTagGroup.GenerateEmpty();
            Ongoing = AvoidRequireTagGroup.GenerateEmpty();
        }

        public EmptyEffectRequirements(AvoidRequireTagGroup application, AvoidRequireTagGroup removal, AvoidRequireTagGroup ongoing)
        {
            Application = application;
            Removal = removal;
            Ongoing = ongoing;
        }

        public bool CheckApplicationRequirements(List<ITag> tags)
        {
            return Application.Validate(tags);
        }
        
        public bool CheckRemovalRequirements(List<ITag> tags)
        {
            return Removal.Validate(tags);
        }
        
        public bool CheckOngoingRequirements(List<ITag> tags)
        {
            return Ongoing.Validate(tags);
        }
    }
}
