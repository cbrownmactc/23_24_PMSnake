using System;
using System.Windows.Media;

namespace PMSnake
{
    public static class Audio
    {
        public readonly static MediaPlayer GameOver =
            LoadAudio("game-over.wav");
        
        private static MediaPlayer LoadAudio(
            string filename, double volume=1, bool repeat = false, bool autoReset = true)
        {
            MediaPlayer player = new();
            player.Open(new Uri($"Assets/{filename}", UriKind.Relative));
            player.Volume = volume;

            if (repeat)
            {
                player.MediaEnded += PlayerRepeat_MediaEnded;
            }

            if (autoReset)
            {
                player.MediaEnded += Player_MediaEnded;
            }

            return player;
        }

        private static void Player_MediaEnded(object sender, EventArgs e)
        {
            MediaPlayer m = sender as MediaPlayer;
            m.Stop();
            m.Position = new TimeSpan(0);
        }

        private static void PlayerRepeat_MediaEnded(object sender, EventArgs e)
        {
            MediaPlayer m = sender as MediaPlayer;
            m.Stop();
            m.Position = new TimeSpan(0);
            m.Play();
        }
    }
}
