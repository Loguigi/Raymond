using DSharpPlus.Entities;
using Raymond.Bot.Discord;
using Raymond.Data.Repositories;
using Raymond.Shared.Contracts;
using Raymond.Shared.Entities;

namespace Raymond.Bot.Services;

public class SecretSantaService(
    ISecretSantaRepository santaRepo)
{
    public async Task<Result<SantaConfig>> StartSecretSantaAsync(
        DiscordGuild guild, 
        DateOnly participationDeadline, 
        DateOnly giftExchange, 
        string location, 
        double? priceLimit)
    {
        var santaConfig = await santaRepo.GetSantaConfigAsync(guild.Id.ToDbId());

        if (santaConfig is not null)
        {
            return Result<SantaConfig>.Failed("Secret Santa event has already started");
        }
        
        if (participationDeadline >= giftExchange)
        {
            return Result<SantaConfig>.Failed("Participation deadline must be before gift exchange date");
        }

        var santaRole = await guild.CreateRoleAsync("Secret Santa", null, DiscordColor.DarkRed);

        santaConfig = new SantaConfig
        {
            GuildId = guild.Id.ToDbId(),
            SantaRoleId = santaRole.Id.ToDbId(),
            ParticipationDeadline = participationDeadline,
            ExchangeDate = giftExchange,
            ExchangeLocation = location,
            PriceLimit = priceLimit,
        };

        await santaRepo.SaveSantaConfigAsync(santaConfig);
        
        return Result<SantaConfig>.Success(santaConfig);
    }
    
    public async Task<Result<SantaConfig>> GetSantaConfigAsync(DiscordGuild guild)
    {
        var santaConfig = await santaRepo.GetSantaConfigAsync(guild.Id.ToDbId());
        if (santaConfig is null)
        {
            return Result<SantaConfig>.Failed("Secret Santa event has not started");
        }

        return Result<SantaConfig>.Success(santaConfig);
    }

    public async Task<Result<CoupleSaveResult>> SaveCoupleAsync(DiscordGuild guild, DiscordUser first,
        DiscordUser second)
    {
        var santaConfig = await santaRepo.GetSantaConfigAsync(guild.Id.ToDbId());
        if (santaConfig is null)
        {
            return Result<CoupleSaveResult>.Failed("Secret Santa event has not started");
        }

        // === Event started check ===
        if (santaConfig.LockedIn)
        {
            return Result<CoupleSaveResult>.Failed("Giftees are already assigned. Cannot change configuration");
        }

        var participants = await santaRepo.GetSantaParticipantsAsync(guild.Id.ToDbId());
            
        // === Valid participant checks ===
        var firstParticipant = participants.FirstOrDefault(m => m.Member.UserId == first.Id.ToDbId());
        if (firstParticipant is null)
        {
            return Result<CoupleSaveResult>.Failed($"{first.Mention} is not a participant in the Secret Santa");
        }
            
        var secondParticipant = participants.FirstOrDefault(m => m.Member.UserId == second.Id.ToDbId());
        if (secondParticipant is null)
        {
            return Result<CoupleSaveResult>.Failed($"{second.Mention} is not a participant in the Secret Santa");
        }
            
        // === Existing SO checks ===

        if (firstParticipant.SignificantOther is not null)
        {
            return Result<CoupleSaveResult>.Failed($"{firstParticipant.Member.DisplayMention()} already has a SO assigned: {firstParticipant.SignificantOther.Mention()}");
        }

        if (secondParticipant.SignificantOther is not null)
        {
            return Result<CoupleSaveResult>.Failed($"{secondParticipant.Member.DisplayMention()} already has a SO assigned: {secondParticipant.SignificantOther.Mention()}");
        }

        firstParticipant.SignificantOther = secondParticipant.Member;
        secondParticipant.SignificantOther = firstParticipant.Member;

        await santaRepo.SaveCoupleAsync(firstParticipant, secondParticipant);

        return Result<CoupleSaveResult>.Success(new CoupleSaveResult(firstParticipant, secondParticipant),
            $"Couple successfully saved: {firstParticipant.Member.DisplayMention()} and {secondParticipant.Member.DisplayMention()} will not get each other in the Secret Santa");

    }
}