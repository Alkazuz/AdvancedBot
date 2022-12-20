namespace AdvancedBot.client.NBT
{
    /**
     * Copyright Mojang AB.
     * 
     * Don't do evil.
     */

    public class EndTag : Tag
    {

        public EndTag()
            : base(null)
        {
        }

        public override void Load(DataInput dis)
        {
        }

        public override void Write(DataOutput dos)
        {
        }

        public override byte GetID()
        {
            return TAG_End;
        }

        public override string ToString()
        {
            return "END";
        }

        public override Tag Copy()
        {
            return new EndTag();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}