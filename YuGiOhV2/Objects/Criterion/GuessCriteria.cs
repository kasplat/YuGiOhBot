﻿using Discord.Addons.Interactive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace YuGiOhV2.Objects.Criterion
{
    public class GuessCriteria : ICriterion<SocketMessage>
    {

        private string _answer;

        public GuessCriteria(string answer)
        {

            _answer = answer;

        }

        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, SocketMessage parameter)
        {

            return Task.FromResult(parameter.Content == _answer);

        }

    }
}
