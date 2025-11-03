using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VoiceAssistant.Server.Domain;
using VoiceAssistant.Server.Redis.Extensions;

namespace VoiceAssistant.Server.Services.Extensions
{
	public static class DIExtensions
	{
		public static IServiceCollection AddLuaScripts(this IServiceCollection services)
		{
			services
				.AddLuaScript(DIConsts.KEY_LUA_MOVE_BACK_TO_PENDING, "VoiceAssistant.Server.Services.Lua.ReliableQueue.MoveBackToPending.lua")
				.AddLuaScript(DIConsts.KEY_LUA_PUSH_RECOGNITION_TASK, "VoiceAssistant.Server.Services.Lua.ReliableQueue.MoveBackToPending.lua");

			return services;
		}
	}
}
