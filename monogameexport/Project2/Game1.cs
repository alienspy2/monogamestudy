using MGAlienLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Project2
{
    public class Game1 : GameBase
    {
        public Game1() : base()
        {
            IsMouseVisible = true;
        }

        protected override void OnExiting(object sender, ExitingEventArgs args)
        {
            base.OnExiting(sender, args);
            Logger.Log("bye!");
        }

        protected override void OnInitialize()
        {
            Logger.Pipe += Logger.PipeToLogFile;

            //var entryObj = hierarchyManager.CreateGameObject("entry", null);
            //entryObj.AddComponent<testEntry>();
            var gameEntry = hierarchyManager.CreateGameObject("GameEntry", null);
            gameEntry.AddComponent<GameEntry>();
        }

    }
}
