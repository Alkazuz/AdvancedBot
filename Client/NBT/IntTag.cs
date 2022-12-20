namespace AdvancedBot.client.NBT
{
    /**
     * Copyright Mojang AB.
     * 
     * Don't do evil.
     */


    public class IntTag : Tag
    {
        public int Data;

        public IntTag(string name)
            : base(name)
        {
        }

        public IntTag(string name, int data)
            : base(name)
        {
            this.Data = data;
        }

        public override void Write(DataOutput dos)
        {
            dos.WriteInt(Data);
        }

        public override void Load(DataInput dis)
        {
            Data = dis.ReadInt();
        }

        public override byte GetID()
        {
            return TAG_Int;
        }

        public override string ToString()
        {
            return "" + Data;
        }

        public override Tag Copy()
        {
            return new IntTag(GetName(), Data);
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                IntTag o = (IntTag)obj;
                return Data == o.Data;
            }
            return false;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)2166136261;
                hash = (hash * 16777619) ^ Data.GetHashCode();
                hash = (hash * 16777619) ^ GetID().GetHashCode();
                hash = (hash * 16777619) ^ GetName().GetHashCode();
                return hash;
            }
        }
    }
}