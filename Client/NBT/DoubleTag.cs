namespace AdvancedBot.client.NBT
{
    /**
     * Copyright Mojang AB.
     * 
     * Don't do evil.
     */

    public class DoubleTag : Tag
    {
        public double Data;

        public DoubleTag(string name)
            : base(name)
        {
            //super(name);
        }

        public DoubleTag(string name, double data)
            : base(name)
        {
            //super(name);
            this.Data = data;
        }

        public override void Write(DataOutput dos)
        {
            dos.WriteDouble(Data);
        }

        public override void Load(DataInput dis)
        {
            Data = dis.ReadDouble();
        }

        public override byte GetID()
        {
            return TAG_Double;
        }

        public override string ToString()
        {
            return "" + Data;
        }

        public override Tag Copy()
        {
            return new DoubleTag(GetName(), Data);
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                DoubleTag o = (DoubleTag)obj;
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