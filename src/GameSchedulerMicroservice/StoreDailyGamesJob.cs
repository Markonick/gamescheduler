﻿using System;
using System.Threading.Tasks;
using GameScheduler.Repositories;
using Quartz;

namespace GameScheduler
{
    public class StoreDailyGamesJob : IJob
    {
        private readonly IGameScheduleRepository _gameRepo;

        public StoreDailyGamesJob(IGameScheduleRepository gameRepo)
        {
            _gameRepo = gameRepo;
        }

        public async Task Execute(IJobExecutionContext context)
        {     
                _gameRepo.StoreDailySchedule();
                Console.WriteLine("Hi from StoreDailyGameJOb");
                await Task.Delay(1);
    
        }
    }
}
