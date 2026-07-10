// See https://aka.ms/new-console-template for more information
using DotNetElevators;

Console.WriteLine(@"
      ___                         ___                        ___                                   ___           ___                    ___           ___           ___           ___           ___     
     /  /\                       /  /\          ___         /  /\          ___       ___          /  /\         /__/\                  /__/\         /  /\         /__/\         /  /\         /  /\    
    /  /:/_                     /  /:/_        /__/\       /  /::\        /  /\     /  /\        /  /::\        \  \:\                 \  \:\       /  /::\       |  |::\       /  /:/_       /  /:/_   
   /  /:/ /\    ___     ___    /  /:/ /\       \  \:\     /  /:/\:\      /  /:/    /  /:/       /  /:/\:\        \  \:\                 \__\:\     /  /:/\:\      |  |:|:\     /  /:/ /\     /  /:/ /\  
  /  /:/ /:/_  /__/\   /  /\  /  /:/ /:/_       \  \:\   /  /:/~/::\    /  /:/    /__/::\      /  /:/  \:\   _____\__\:\            ___ /  /::\   /  /:/  \:\   __|__|:|\:\   /  /:/ /:/_   /  /:/ /::\ 
 /__/:/ /:/ /\ \  \:\ /  /:/ /__/:/ /:/ /\  ___  \__\:\ /__/:/ /:/\:\  /  /::\    \__\/\:\__  /__/:/ \__\:\ /__/::::::::\          /__/\  /:/\:\ /__/:/ \__\:\ /__/::::| \:\ /__/:/ /:/ /\ /__/:/ /:/\:\
 \  \:\/:/ /:/  \  \:\  /:/  \  \:\/:/ /:/ /__/\ |  |:| \  \:\/:/__\/ /__/:/\:\      \  \:\/\ \  \:\ /  /:/ \  \:\~~\~~\/          \  \:\/:/__\/ \  \:\ /  /:/ \  \:\~~\__\/ \  \:\/:/ /:/ \  \:\/:/~/:/
  \  \::/ /:/    \  \:\/:/    \  \::/ /:/  \  \:\|  |:|  \  \::/      \__\/  \:\      \__\::/  \  \:\  /:/   \  \:\  ~~~            \  \::/       \  \:\  /:/   \  \:\        \  \::/ /:/   \  \::/ /:/ 
   \  \:\/:/      \  \::/      \  \:\/:/    \  \:\__|:|   \  \:\           \  \:\     /__/:/    \  \:\/:/     \  \:\                 \  \:\        \  \:\/:/     \  \:\        \  \:\/:/     \__\/ /:/  
    \  \::/        \__\/        \  \::/      \__\::::/     \  \:\           \__\/     \__\/      \  \::/       \  \:\                 \  \:\        \  \::/       \  \:\        \  \::/        /__/:/   
     \__\/                       \__\/           ~~~~       \__\/                                 \__\/         \__\/                  \__\/         \__\/         \__\/         \__\/         \__\/    
");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<BuildingService>();
builder.Services.AddSingleton<QueueManager>();
builder.Services.AddSingleton<PassengerService>();

builder.Services.AddHostedService<ElevatorManagementService>();
builder.Services.AddHostedService<PassengerTimer>();
builder.Services.AddControllers();

using var app = builder.Build();

app.MapControllers();

await app.RunAsync();