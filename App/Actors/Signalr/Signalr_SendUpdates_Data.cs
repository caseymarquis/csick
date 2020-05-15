using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSick.Actors.Signalr {
    public struct Update {
        public string Group;
        public string Cmd;
    }

    public interface IUpdateHubClient {
        Task receiveUpdates(List<string> updateCommands);
    }

    public class UpdateHub : Hub<IUpdateHubClient> {
        public async Task renew(string groups) {
            foreach (var group in groups.Split('|')) {
                await this.Groups.AddToGroupAsync(this.Context.ConnectionId, group);
            }
        }

        public async Task subscribe(string group) {
            await this.Groups.AddToGroupAsync(this.Context.ConnectionId, group);
        }

        public async Task unsubscribe(string group) {
            await this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, group);
        }
    }
}
