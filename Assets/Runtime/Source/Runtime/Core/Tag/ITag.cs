using System.Collections.Generic;

namespace FESGameplayAbilitySystem
{
    public interface ITag
    {
        private static int nextFree = 0;
        private static int last = 0;

        public static IntegerTag Create()
        {
            return Get(nextFree);
        }

        public static IntegerTag Get(int key)
        {
            int _key = key;
            while (!Tags.TagIsAvailable(_key)) _key += 1;
            
            if (_key >= nextFree) nextFree = _key + 1;
            last = _key;
            return new IntegerTag(_key);
        }
        
        /// <summary>
        /// Bypasses the safety check with respect to reserved tag channels.
        /// </summary>
        /// <param name="key">The tag's key</param>
        /// <returns>The created tag</returns>
        public static IntegerTag GetUnsafe(int key)
        {
            if (key >= nextFree) nextFree = key + 1;
            last = key;
            return new IntegerTag(key);
        }

        public static IntegerTag Last()
        {
            return new IntegerTag(last);
        }

        public bool Equals(ITag other);
    }
    
    public class IntegerTag : ITag
    {
        private int key;
        
        public IntegerTag(int key)
        {
            this.key = key;
        }

        public bool Equals(ITag other)
        {
            var tag = (IntegerTag)other;
            return tag is not null && tag.key == key;
        }
    }

    public class TagComparer : IEqualityComparer<ITag>
    {
        public bool Equals(ITag x, ITag y)
        {
            return x != null && x.Equals(y);
        }
        public int GetHashCode(ITag obj)
        {
            return obj.GetHashCode();
        }
    }
}
