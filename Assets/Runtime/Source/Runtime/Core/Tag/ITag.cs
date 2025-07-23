using System.Collections.Generic;

namespace FESGameplayAbilitySystem
{
    public interface ITag
    {
        public static CustomTag Create(int key = 0)
        {
            return new CustomTag(key);
        }

        public bool Compare(ITag other);
    }
    
    public class CustomTag : ITag
    {
        private int key;
        public CustomTag(int key)
        {
            this.key = key;
        }

        public bool Compare(ITag other)
        {
            var tag = (CustomTag)other;
            return tag is not null && tag.key == key;
        }
    }

    public class TagComparer : IEqualityComparer<ITag>
    {
        public bool Equals(ITag x, ITag y)
        {
            return x != null && x.Compare(y);
        }
        public int GetHashCode(ITag obj)
        {
            return obj.GetHashCode();
        }
    }
}
