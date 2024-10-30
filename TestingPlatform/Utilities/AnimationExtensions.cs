namespace TestingPlatform.Utilities
{
    public static class AnimationExtensions
    {
        public static Task<bool> WidthTo(this VisualElement element, double newWidth, uint length, Easing easing)
        {
            return element.AnimateAsync("WidthTo", (progress) =>
            {
                element.WidthRequest = element.Width + (newWidth - element.Width) * progress;
            }, length, easing);
        }

        private static Task<bool> AnimateAsync(this VisualElement element, string name, Action<double> callback, uint length, Easing easing)
        {
            TaskCompletionSource<bool> taskCompletionSource = new();

            element.Animate(name, callback, length: length, easing: easing, finished: (v, c) => taskCompletionSource.SetResult(c));

            return taskCompletionSource.Task;
        }
    }
}