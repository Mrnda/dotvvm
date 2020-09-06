import * as uri from '../utils/uri';
import * as http from '../postback/http';
import { getViewModel } from '../dotvvm-base';
import * as events from '../events';
import { navigateCore } from './navigation';
import * as counter from '../postback/counter';
import { options } from 'knockout';

export const isSpaReady = ko.observable(false);

export function init(): void {
    const spaPlaceHolder = getSpaPlaceHolder();
    if (spaPlaceHolder == null) {
        throw new Error("The SpaContentPlaceHolder control was not found!");
    }

    window.addEventListener("hashchange", event => handleHashChangeWithHistory(spaPlaceHolder, false));
    handleHashChangeWithHistory(spaPlaceHolder, true);

    window.addEventListener('popstate', event => handlePopState(event, spaPlaceHolder != null));
}

function getSpaPlaceHolder(): HTMLElement | null {
    const elements = document.getElementsByName("__dot_SpaContentPlaceHolder");
    if (elements.length == 1) {
        return <HTMLElement> elements[0];
    }
    return null;
}

export function getSpaPlaceHolderUniqueId(): string {
    return getSpaPlaceHolder()!.getAttribute("data-dotvvm-spacontentplaceholder")!;
}

function handlePopState(event: PopStateEvent, inSpaPage: boolean) {
    if (isSpaPage(event.state)) {
        const historyRecord = <HistoryRecord> (event.state);
        if (inSpaPage) {
            handleSpaNavigationCore(historyRecord.url);
        } else {
            location.replace(historyRecord.url);
        }

        event.preventDefault();
    }
}

function handleHashChangeWithHistory(spaPlaceHolder: HTMLElement, isInitialPageLoad: boolean) {
    if (document.location.hash.indexOf("#!/") === 0) {
        // the user requested navigation to another SPA page
        handleSpaNavigationCore(
            document.location.hash.substring(2),
            (url) => { replacePage(url); }
        );
    } else {
        isSpaReady(true);
        spaPlaceHolder.style.display = "";

        const currentRelativeUrl = location.pathname + location.search + location.hash
        replacePage(currentRelativeUrl);
    }
}

export async function handleSpaNavigation(element: HTMLElement): Promise<DotvvmNavigationEventArgs | undefined> {
    const target = element.getAttribute('target');
    if (target == "_blank") {
        return;     // TODO: shall we return result if the target is _blank? And what about other targets?
    }

    return await handleSpaNavigationCore(element.getAttribute('href'));
}

export async function handleSpaNavigationCore(url: string | null, handlePageNavigating?: (url: string) => void): Promise<DotvvmNavigationEventArgs> {

    if (!url || url.indexOf("/") !== 0) {
        throw new Error("Invalid url for SPAN navigation!");
    }

    const currentPostBackCounter = counter.backUpPostBackCounter();

    const options: PostbackOptions = {
        commandType: "spaNavigation",
        postbackId: currentPostBackCounter,
        args: []
    };

    try {

        url = uri.removeVirtualDirectoryFromUrl(url);
        return await navigateCore(url, options, handlePageNavigating || defaultHandlePageNavigating);

    } catch (err) {

        // execute error handler
        const errArgs: DotvvmErrorEventArgs = {
            ...options,
            error: err,
            response: (err as any).response,
            serverResponseObject: (err as any).result,
            handled: false
        };
        events.error.trigger(errArgs);
        if (!errArgs.handled) {
            console.error("SPA Navigation Error", errArgs);
        }
        throw err;
    }
}

function defaultHandlePageNavigating(navigatedUrl: string) {
    if (!history.state || history.state.url != navigatedUrl) {
        pushPage(navigatedUrl);
    }
}

class HistoryRecord {
    constructor(public navigationType: string, public url: string) { }
}

function pushPage(url: string): void {
    // pushState doesn't work when the url is empty
    url = url || "/";
    
    history.pushState(new HistoryRecord('SPA', url), '', url);
}

function replacePage(url: string): void {
    history.replaceState(new HistoryRecord('SPA', url), '', url);
}

function isSpaPage(state: any): boolean {
    return state && state.navigationType == 'SPA';
}
