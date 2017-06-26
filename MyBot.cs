using System;

using Discord;
using Discord.Commands;
using Discord.Audio;

using NAudio.Wave;

namespace TheBot
{
    class MyBot
    {
        //DiscordClient discord;
        DiscordClient discord = new DiscordClient(x =>
        {
            x.LogLevel = LogSeverity.Info;
            //x.LogHandler = Log;
        });

        CommandService commands;

        Random rand;

        string[] freshmemes;

        string[] hello;

        string txt = System.IO.File.ReadAllText("mems/pasta.txt");

        public MyBot()
        {B
            rand = new Random();
			
			//array of picture locations
            freshmemes = new string[]
            {
                "mems/mem1.jpg",
                "mems/mem2.jpg",
                "mems/mem3.png"
            };
			//array of hello strings
            hello = new string[]
            {
                "suh dude",
                "hi",
                "hey xD",
                "hello",
                "no go away"
            };


            discord.Log.Message += (s, e) => Console.WriteLine($"[{e.Severity}] {e.Source}: {e.Message}");
			
			//
            discord.UsingAudio(x => // Opens an AudioConfigBuilder so we can configure our AudioService
            {
                x.Mode = AudioMode.Outgoing; // Tells the AudioService that we will only be sending audio
            });
			
			//set the command prefix for the bot
            discord.UsingCommands(x =>
            {
                x.PrefixChar = '!';
                x.AllowMentionPrefix = true;
            });

			//initialize a command service
            commands = discord.GetService<CommandService>();

            RegisterCommands();
			
			//connect the bot to the Discord server
            discord.ExecuteAndWait(async () =>
            {
                await discord.Connect("Server code goes here", TokenType.Bot);
            });
        }

        private void RegisterCommands()
        {
			//posts a meme pic from local folder
            commands.CreateCommand("meme")
                .Do(async (e) =>
                {
                    int randomMemeIndex = rand.Next(freshmemes.Length);
                    string memToPost = freshmemes[randomMemeIndex];
                    await e.Channel.SendFile(memToPost);
                });
			//responds with a variation of hi
            commands.CreateCommand("hello")
                .Do(async (e) =>
                {
                    int randomHelloIndex = rand.Next(hello.Length);
                    await e.Channel.SendMessage(hello[randomHelloIndex]);
                });
			//posts the copypasta written in the txt file
            commands.CreateCommand("pasta")
                .Do(async (e) =>
                {

                    await e.Channel.SendMessage(txt);
                });
			//clears the last 100 messages in the server
            commands.CreateCommand("purge")
                .Do(async (e) =>
                {
                    Message[] messagesToDelete;
                    messagesToDelete = await e.Channel.DownloadMessages(100);
                    await e.Channel.DeleteMessages(messagesToDelete);
                });
			//joins the channel of the user that sent the command
            commands.CreateCommand("join")
                .Do(async (e) =>
                {
                    //var voiceChannel = discord.FindServers("no memes allowed").FirstOrDefault().VoiceChannels.FirstOrDefault(); // Finds the first VoiceChannel on the server 'Music Bot Server'

                    var _vClient = await discord.GetService<AudioService>().Join(e.User.VoiceChannel);
                    //var _vClient = await discord.GetService<AudioService>() // We use GetService to find the AudioService that we installed earlier. In previous versions, this was equivelent to _client.Audio()
                    //.Join(voiceChannel); // Join the Voice Channel, and return the IAudioClient.
                });
			//leaves the current channel
            commands.CreateCommand("leave")
                .Do(async (e) =>
                {
                    await discord.GetService<AudioService>().Leave(e.User.VoiceChannel);

                });
			//plays a local music file over the bots 'mic' in the current channel
			//ideally will eventually stream music through youtube or soundcloud links
            commands.CreateCommand("music q")
                .Parameter("qwerty", ParameterType.Multiple)
                .Do(async (e) =>
                {
                    Console.WriteLine("1");
                    Channel voiceChannel = e.User.VoiceChannel;
                    Console.WriteLine(voiceChannel);
                    var _vClient = await discord.GetService<AudioService>().Join(e.User.VoiceChannel);

                    var channelCount = discord.GetService<AudioService>().Config.Channels; // Get the number of AudioChannels our AudioService has been configured to use.
                    var OutFormat = new WaveFormat(48000, 16, channelCount); // Create a new Output Format, using the spec that Discord will accept, and with the number of channels that our client supports.
                    using (var MP3Reader = new Mp3FileReader(@"Music/01.mp3")) // Create a new Disposable MP3FileReader, to read audio from the filePath parameter
                    using (var resampler = new MediaFoundationResampler(MP3Reader, OutFormat)) // Create a Disposable Resampler, which will convert the read MP3 data to PCM, using our Output Format
                    {
                        resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality
                        int blockSize = OutFormat.AverageBytesPerSecond / 50; // Establish the size of our AudioBuffer
                        byte[] buffer = new byte[blockSize];
                        int byteCount;
                        Console.WriteLine("2");
                        while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0) // Read audio into our buffer, and keep a loop open while data is present
                        {
                            if (byteCount < blockSize)
                            {
                                Console.WriteLine("3");
                                // Incomplete Frame
                                for (int i = byteCount; i < blockSize; i++)
                                    buffer[i] = 0;
                            }
                            _vClient.Send(buffer, 0, blockSize); // Send the buffer to Discord
                        }
                    }/*
                    using (var MP3Reader = new Mp3FileReader(@"Music/02.mp3")) // Create a new Disposable MP3FileReader, to read audio from the filePath parameter
                    using (var resampler = new MediaFoundationResampler(MP3Reader, OutFormat)) // Create a Disposable Resampler, which will convert the read MP3 data to PCM, using our Output Format
                    {
                        resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality
                        int blockSize = OutFormat.AverageBytesPerSecond / 50; // Establish the size of our AudioBuffer
                        byte[] buffer = new byte[blockSize];
                        int byteCount;
                        Console.WriteLine("2");
                        while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0) // Read audio into our buffer, and keep a loop open while data is present
                        {
                            if (byteCount < blockSize)
                            {
                                Console.WriteLine("3");
                                // Incomplete Frame
                                for (int i = byteCount; i < blockSize; i++)
                                    buffer[i] = 0;
                            }
                            _vClient.Send(buffer, 0, blockSize); // Send the buffer to Discord
                        }
                    }*/
                });
        }
        /*
        public void SendAudio(string pathOrUrl)
        {
            var _vClient = discord.GetService<AudioService>();
            
            var process = Process.Start(new ProcessStartInfo
            { // FFmpeg requires us to spawn a process and hook into its stdout, so we will create a Process
                FileName = "ffmpeg",
                Arguments = $"-i {pathOrUrl} " + // Here we provide a list of arguments to feed into FFmpeg. -i means the location of the file/URL it will read from
                            "-f s16le -ar 48000 -ac 2 pipe:1", // Next, we tell it to output 16-bit 48000Hz PCM, over 2 channels, to stdout.
                UseShellExecute = false,
                RedirectStandardOutput = true // Capture the stdout of the process
            });
            Thread.Sleep(2000); // Sleep for a few seconds to FFmpeg can start processing data.

            int blockSize = 3840; // The size of bytes to read per frame; 1920 for mono
            byte[] buffer = new byte[blockSize];
            int byteCount;

            while (true) // Loop forever, so data will always be read
            {
                byteCount = process.StandardOutput.BaseStream // Access the underlying MemoryStream from the stdout of FFmpeg
                        .Read(buffer, 0, blockSize); // Read stdout into the buffer

                if (byteCount == 0) // FFmpeg did not output anything
                    break; // Break out of the while(true) loop, since there was nothing to read.

                _vClient.Send(buffer, 0, byteCount); // Send our data to Discord
                
                _vClient.
            }
            _vClient.Wait(); // Wait for the Voice Client to finish sending data, as ffMPEG may have already finished buffering out a song, and it is unsafe to return now.
        }*/
        /*
            commands.CreateCommand("music2 q")
                //.Parameter("qwerty", ParameterType.Multiple)
                .Do(async (e) =>
                {
                    var _vClient = await discord.GetService<AudioService>().Join(e.User.VoiceChannel);

                    var process = Process.Start(new ProcessStartInfo
                    { // FFmpeg requires us to spawn a process and hook into its stdout, so we will create a Process
                        FileName = "ffmpeg",
                        Arguments = $"-i {"https://www.youtube.com/watch?v=i3xY3tS_59k"} " + // Here we provide a list of arguments to feed into FFmpeg. -i means the location of the file/URL it will read from
                                    "-f s16le -ar 48000 -ac 2 pipe:1", // Next, we tell it to output 16-bit 48000Hz PCM, over 2 channels, to stdout.
                        UseShellExecute = false,
                        RedirectStandardOutput = true // Capture the stdout of the process
                    });
                    Thread.Sleep(2000); // Sleep for a few seconds to FFmpeg can start processing data.

                    int blockSize = 3840; // The size of bytes to read per frame; 1920 for mono
                    byte[] buffer = new byte[blockSize];
                    int byteCount;

                    while (true) // Loop forever, so data will always be read
                    {
                        byteCount = process.StandardOutput.BaseStream // Access the underlying MemoryStream from the stdout of FFmpeg
                                .Read(buffer, 0, blockSize); // Read stdout into the buffer

                        if (byteCount == 0) // FFmpeg did not output anything
                            break; // Break out of the while(true) loop, since there was nothing to read.

                        _vClient.Send(buffer, 0, byteCount); // Send our data to Discord
                    }
                    _vClient.Wait();
                });*/

        private void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }
    }
}