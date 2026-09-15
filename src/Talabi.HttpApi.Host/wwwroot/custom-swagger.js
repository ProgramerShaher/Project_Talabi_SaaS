window.addEventListener("load", function () {
    let isScriptClick = false;

    document.addEventListener("click", function (event) {
        if (isScriptClick) return;

        // Check if the click is on an opblock-tag
        const clickedTag = event.target.closest('.opblock-tag');
        
        if (clickedTag) {
            // Find elements with aria-expanded="true" to close other groups
            const expandedElements = document.querySelectorAll('[aria-expanded="true"]');
            
            expandedElements.forEach(function (el) {
                const tagToClose = el.closest('.opblock-tag');
                if (tagToClose && tagToClose !== clickedTag) {
                    isScriptClick = true;
                    tagToClose.click();
                    isScriptClick = false;
                }
            });
            
            // Fallback for Swagger versions that use data-is-open on the tag
            const openTags = document.querySelectorAll('.opblock-tag[data-is-open="true"]');
            openTags.forEach(function (tagToClose) {
                if (tagToClose && tagToClose !== clickedTag) {
                    isScriptClick = true;
                    tagToClose.click();
                    isScriptClick = false;
                }
            });
            
            return; // Finished handling tag click
        }
        
        // Check if the click is on an opblock-summary (Endpoint)
        const clickedEndpoint = event.target.closest('.opblock-summary');
        
        if (clickedEndpoint) {
            // Find all open endpoints (with class is-open on their parent opblock)
            const openEndpoints = document.querySelectorAll('.opblock.is-open .opblock-summary');
            
            openEndpoints.forEach(function (summaryToClose) {
                if (summaryToClose && summaryToClose !== clickedEndpoint) {
                    isScriptClick = true;
                    summaryToClose.click();
                    isScriptClick = false;
                }
            });
            
            // Fallback for versions using aria-expanded directly without is-open class on parent
            const expandedSummaries = document.querySelectorAll('.opblock-summary[aria-expanded="true"], .opblock-summary [aria-expanded="true"]');
            expandedSummaries.forEach(function (el) {
                const summaryToClose = el.closest('.opblock-summary');
                // Check if it's not the clicked one, and we haven't already closed it in the previous loop
                if (summaryToClose && summaryToClose !== clickedEndpoint && !summaryToClose.closest('.opblock.is-open')) {
                    isScriptClick = true;
                    summaryToClose.click();
                    isScriptClick = false;
                }
            });
        }
    });
});
