namespace AdvancedBot.client.NBT
{
    /**
     * Copyright Mojang AB.
     * 
     * Don't do evil.
     */

    public class FloatTag : Tag
    {
        public float Data;

        public FloatTag(string name)
            : base(name)
        {
        }

        public FloatTag(string name, float data)
            : base(name)
        {
            this.Data = data;
        }

        public override void Write(DataOutput dos)
        {
            dos.WriteFloat(Data);
        }

        public override void Load(DataInput dis)
        {
            Data = dis.ReadFloat();
        }

        public override byte GetID()
        {
            return TAG_Float;
        }

        public override string ToString()
        {
            return "" + Data;
        }

        public override Tag Copy()
        {
            return new FloatTag(GetName(), Data);
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                FloatTag o = (FloatTag)obj;
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