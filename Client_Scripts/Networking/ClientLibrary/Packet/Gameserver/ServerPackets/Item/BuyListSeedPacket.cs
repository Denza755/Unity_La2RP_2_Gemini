public class BuyListSeedPacket : ServerPacket
{
    public int Adena { get; private set; }
    public int ListId { get; private set; }
    public Product[] Products { get; private set; }

    public BuyListSeedPacket(byte[] d) : base(d)
    {
        Parse();
    }

    public override void Parse()
    {
        Adena = ReadI();
        ListId = ReadI();
        int count = ReadH();

        Products = new Product[count];
        for (int i = 0; i < count; i++)
        {
            Products[i] = new Product();
            Products[i].Type1 = (ItemType1)ReadH();
            Products[i].ItemId = ReadI();
            ReadI(); // objectId
            Products[i].Count = ReadI();
            Products[i].Type2 = (ItemType2)ReadH();
            ReadH(); // 0
            Products[i].Price = ReadI();
        }
    }
}
