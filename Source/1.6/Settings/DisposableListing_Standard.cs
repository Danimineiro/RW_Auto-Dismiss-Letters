using System;
using Verse;

namespace DismissLetters.Settings;
internal class DisposableListing_Standard : Listing_Standard, IDisposable
{
    public void Dispose()
    {
        End();
        GC.SuppressFinalize(this);
    }
}
