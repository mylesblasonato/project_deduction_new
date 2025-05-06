using System;
using LoadingScreenKit.Scripts;
using UnityEngine;
using UnityEngine.UIElements;

namespace LoadingScreenKit.Components.Texts
{
    [UxmlElement]
    public partial class QuoteLabel : Label
    {
        [UxmlAttribute]
        [SerializeField] private QuoteSource quotes;

        [UxmlAttribute]
        [SerializeField] private float delayEnabled = 1;

        [UxmlAttribute]
        [SerializeField] private float delayDisabled = 1;

        [UxmlAttribute]
        [SerializeField] private bool startDisabled;

        [UxmlAttribute]
        [SerializeField] private bool randomizeQuotes;

        private IVisualElementScheduledItem scheduler;
        private int currentQuoteIndex;
        private bool isTransitioning;

        #region Initialization

        public QuoteLabel()
        {
            RegisterCallbackOnce<AttachToPanelEvent>(OnAttach);
            RegisterCallbackOnce<DetachFromPanelEvent>(OnDetach);
        }

        private void OnAttach(AttachToPanelEvent evt)
        {
            if (!AreQuotesValid())
            {
                text = "No quotes available.";
                return;
            }

            currentQuoteIndex = randomizeQuotes ? GetRandomIndex() : 0;

            if (!startDisabled)
                TransitionToQuote(() => currentQuoteIndex);
            else
                Show();
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            StopScheduler();
        }

        #endregion

        #region Quote Management

        private bool AreQuotesValid() =>
            quotes != null && quotes.quotes != null && quotes.quotes.Length > 0;

        private string GetCurrentQuote() =>
            quotes.quotes[currentQuoteIndex];

        private int GetRandomIndex()
        {
            if (quotes.quotes.Length <= 1) return 0;

            int randomIndex;

            do randomIndex = UnityEngine.Random.Range(0, quotes.quotes.Length);
            while (randomIndex == currentQuoteIndex);

            return randomIndex;
        }

        public void Next()
        {
            if (!AreQuotesValid() || isTransitioning) return;

            TransitionToQuote(() => currentQuoteIndex + 1);
        }

        public void Previous()
        {
            if (!AreQuotesValid() || isTransitioning) return;

            TransitionToQuote(() => currentQuoteIndex - 1 + quotes.quotes.Length);
        }

        private void TransitionToQuote(Func<int> indexCalculation)
        {
            if (isTransitioning) return;

            isTransitioning = true;

            SetEnabled(false);

            currentQuoteIndex = randomizeQuotes
                ? GetRandomIndex()
                : indexCalculation() % quotes.quotes.Length;

            ScheduleAction(Show, delayDisabled);
        }

        #endregion

        #region Visibility and Scheduling

        private void Show()
        {
            SetEnabled(true);

            text = GetCurrentQuote();

            ScheduleAction(() => TransitionToQuote(() => currentQuoteIndex + 1), delayEnabled);

            isTransitioning = false;
        }

        private void ScheduleAction(Action action, float delay)
        {
            StopScheduler();
            scheduler = schedule.Execute(action).StartingIn((long)(delay * 1000));
        }

        private void StopScheduler()
        {
            scheduler?.Pause();
            scheduler = null;
        }

        #endregion
    }
}
