namespace MulTUNG.UI
{
    public interface IDialog
    {
        bool Visible { get; set; }

        void Draw();
    }
}
