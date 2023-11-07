namespace TrickModule.Game
{
    public interface IMenuService
    {
        void ExecuteInit(UIMenu menu);
        void ExecuteShow(UIMenu menu);
        void ExecuteHide(UIMenu menu);
    }
}