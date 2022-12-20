namespace AdvancedBot.client.NBT
{
    /**
     * Copyright Mojang AB.
     * 
     * Don't do evil.
     */

    public class ByteTag : Tag
    {
        public byte Data;

        public ByteTag(string name)
            : base(name)
        {
            // super(name);
        }

        public ByteTag(string name, byte data)
            : base(name)
        {
            //  super(name);
            this.Data = data;
        }

        public override void Write(DataOutput dos)
        {
            dos.WriteByte(Data);
        }

        public override void Load(DataInput dis)
        {
            Data = dis.ReadByte();
        }

        public override byte GetID()
        {
            return TAG_Byte;
        }

        public override string ToString()
        {
            return "" + Data;
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                ByteTag o = (ByteTag)obj;
                return Data == o.Data;
            }
            return false;
        }

        public override Tag Copy()
        {
            return new ByteTag(GetName(), Data);
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