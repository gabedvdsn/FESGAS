using System.Collections.Generic;

namespace FESGameplayAbilitySystem
{
    public interface ITag
    {
        public static IntegerTag Create(int key = 0)
        {
            return new IntegerTag(key);
        }

        public bool Compare(ITag other);
    }
    
    public class IntegerTag : ITag
    {
        private int key;
        public IntegerTag(int key)
        {
            this.key = key;
        }

        public bool Compare(ITag other)
        {
            var tag = (IntegerTag)other;
            return tag is not null && tag.key == key;
        }
    }

    public class StringTag : ITag
    {
        private string key;
        public StringTag(string key)
        {
            this.key = key;
        }
        public bool Compare(ITag other)
        {
            var tag = (StringTag)other;
            return tag is not null && tag.key == key;

        }
    }

    public class SequenceTag : ITag
    {
        private ITag key;
        private SequenceTag Next;
        public SequenceTag(ITag key, SequenceTag next)
        {
            this.key = key;
            Next = next;
        }
        public void Append(SequenceTag other)
        {
            if (Next is null) Next = other;
            else Next.Append(other);
        }
        public bool Compare(ITag other)
        {
            var tag = (SequenceTag)other;
            if (tag is null && Next is null)
            {
                return key.Compare(other);
            }
            return tag is not null && InternalCompare(tag);
        }

        private bool InternalCompare(SequenceTag other)
        {
            bool comp = other.key == key;
            if (!comp) return false;
            if (Next is not null && other.Next is not null) return Next.Compare(other.Next);
            return Next is null && other.Next is null;
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
