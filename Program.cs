// See https://aka.ms/new-console-template for more information
using DotNetElevators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddScoped<BuildingService>();
builder.Services.AddSingleton<QueueManager>();

builder.Services.AddHostedService<ElevatorManagementService>();
builder.Services.AddHostedService<PassengerService>();

using var host = builder.Build();

await host.RunAsync();