using DownloaderBot.Shared.Services;
using DownloaderBot.Worker.Models;

using FluentValidation;

namespace DownloaderBot.Worker.Pipeline.Steps;

public class ValidationStep(IValidator<VideoInfo> validator, IBotResponseService responseService) : IProcessingStep
{
    public async Task ExecuteAsync(ProcessingContext processingContext)
    {
        var info = processingContext.VideoInfo;
        if (info == null)
        {
            return;
        }

        var result = await validator.ValidateAsync(info);

        if (!result.IsValid)
        {
            var errorMsg = result.Errors.First().ErrorMessage;

            await responseService.EditMessageAsync(
                processingContext.Task.ChatId,
                processingContext.Task.StatusMessageId,
                errorMsg);

            processingContext.ShouldStop = true;
        }
    }
}