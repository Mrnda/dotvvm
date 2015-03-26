﻿/// <reference path="typings/knockout/knockout.d.ts" />
/// <reference path="typings/knockout.mapper/knockout.mapper.d.ts" />
/// <reference path="typings/globalize/globalize.d.ts" />

interface RedwoodExtensions {
}
interface RedwoodViewModel {
    viewModel: any;
}

class Redwood { 

    public extensions: RedwoodExtensions = {};
    public viewModels: { [name: string]: RedwoodViewModel } = {};
    private postBackCounter = 0;
    public culture: string;
    public events = {
        preinit: new RedwoodEvent<RedwoodEventArgs>("redwood.events.preinit"),
        init: new RedwoodEvent<RedwoodEventArgs>("redwood.events.init", true),
        beforePostback: new RedwoodEvent<RedwoodBeforePostBackEventArgs>("redwood.events.beforePostback"),
        afterPostback: new RedwoodEvent<RedwoodAfterPostBackEventArgs>("redwood.events.afterPostback"),
        error: new RedwoodEvent<RedwoodErrorEventArgs>("redwood.events.error")
    };

    public includePathProps(viewModel: any, path: string[] = []) {
        viewModel.$path = path;
        for (var p in viewModel) {
            if (typeof viewModel[p] === "object" && viewModel[p] != null && p.charAt(0) != "$") {
                if (viewModel[p] instanceof Array) viewModel[p].forEach((v, i) => this.includePathProps(v, path.concat([p, "[" + i + "]"])))
                else this.includePathProps(viewModel[p], path.concat(p));
            }
        }
    }

    public init(viewModelName: string, culture: string): void {
        this.culture = culture;
        this.includePathProps(this.viewModels[viewModelName].viewModel);

        var viewModel = this.viewModels[viewModelName].viewModel = ko.mapper.fromJS(this.viewModels[viewModelName].viewModel);
        this.events.preinit.trigger(new RedwoodEventArgs(viewModel));

        ko.applyBindings(viewModel);
        this.events.init.trigger(new RedwoodEventArgs(viewModel));
    }
    
    public postBack(viewModelName: string, sender: HTMLElement, path: string[], command: string, controlUniqueId: string, validationTargetPath?: string[]): void {
        var viewModel = this.viewModels[viewModelName].viewModel;
        this.updateDynamicPathFragments(sender, path);

        // prevent double postbacks
        this.postBackCounter++;
        var currentPostBackCounter = this.postBackCounter;

        // trigger beforePostback event
        var beforePostbackArgs = new RedwoodBeforePostBackEventArgs(sender, viewModel, viewModelName, validationTargetPath || ["$this"], path, command);
        this.events.beforePostback.trigger(beforePostbackArgs);
        if (beforePostbackArgs.cancel) {
            return;
        }

        // perform the postback
        var data = {
            viewModel: ko.mapper.toJS(viewModel),
            currentPath: path,
            command: command,
            controlUniqueId: controlUniqueId,
            validationTargetPath: validationTargetPath || null
        };
        this.postJSON(document.location.href, "POST", ko.toJSON(data), result => {
            // if another postback has already been passed, don't do anything
            if (this.postBackCounter !== currentPostBackCounter) return;

            var resultObject = JSON.parse(result.responseText);

            var isSuccess = false;
            if (resultObject.action === "successfulCommand") {
                // remove updated controls
                var updatedControls = {};
                for (var id in resultObject.updatedControls) {
                    if (resultObject.updatedControls.hasOwnProperty(id)) {
                        var control = document.getElementById(id);
                        var nextSibling = control.nextSibling;
                        var parent = control.parentNode;
                        ko.removeNode(control);
                        updatedControls[id] = { control: control, nextSibling: nextSibling, parent: parent };
                    }
                }
                this.includePathProps(resultObject.viewModel);
                // update the viewmodel
                ko.mapper.fromJS(resultObject.viewModel, {}, this.viewModels[viewModelName].viewModel);
                isSuccess = true;

                // add updated controls
                for (id in resultObject.updatedControls) {
                    if (resultObject.updatedControls.hasOwnProperty(id)) {
                        var updatedControl = updatedControls[id];
                        if (updatedControl.nextSibling) {
                            updatedControl.parent.insertBefore(updatedControl.control, updatedControl.nextSibling);
                        } else {
                            updatedControl.parent.appendChild(updatedControl.control);
                        }
                        updatedControl.control.outerHTML = resultObject.updatedControls[id];
                        ko.applyBindings(ko.dataFor(updatedControl.parent), updatedControl.control);
                    }
                }

            } else if (resultObject.action === "redirect") {
                // redirect
                document.location.href = resultObject.url;
                return;
            } 
            
            // trigger afterPostback event
            var afterPostBackArgs = new RedwoodAfterPostBackEventArgs(sender, viewModel, viewModelName, validationTargetPath, resultObject);
            this.events.afterPostback.trigger(afterPostBackArgs);
            if (!isSuccess && !afterPostBackArgs.isHandled) {
                throw "Invalid response from server!";
            }
        }, xhr => {
            // if another postback has already been passed, don't do anything
            if (this.postBackCounter !== currentPostBackCounter) return;

            // execute error handlers
            if (!this.events.error.trigger(new RedwoodErrorEventArgs(viewModel, xhr))) {
                alert(xhr.responseText);
            }
        });
    }

    public formatString(format: string, value: any) {
        if (format == "g") {
            return redwood.formatString("d", value) + " " + redwood.formatString("t", value);
        } else if (format == "G") {
            return redwood.formatString("d", value) + " " + redwood.formatString("T", value);
        }

        value = ko.unwrap(value);
        if (typeof value === "string" && value.match("^[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}(\\.[0-9]{1,3})?$")) {
            // JSON date in string
            value = new Date(value);
        }
        return Globalize.format(value, format, redwood.culture);
    }

    public getDataSourceItems(viewModel: any) {
        var value = ko.unwrap(viewModel);
        return value.Items || value;
    }

    private updateDynamicPathFragments(sender: HTMLElement, path: string[]): void {
        var context = ko.contextFor(sender);

        for (var i = path.length - 1; i >= 0; i--) {
            if (path[i].indexOf("[$index]") >= 0) {
                path[i] = path[i].replace("[$index]", "[" + context.$index() + "]");
            }
            context = context.$parentContext;
        }
    }

    public spitPath(path: string): string[] {
        var indexPos = path.indexOf('[');
        var dotPos = path.indexOf('.');
        var res: string[] = [];
        while (dotPos >= 0 || indexPos >= 0) {
            if (dotPos >= 0 && dotPos < indexPos) {
                res.push(path.substr(0, dotPos));
                path = path.substr(dotPos + 1);
                dotPos = path.indexOf('.');
            }
            if (indexPos >= 0 && indexPos < dotPos) {
                res.push(path.substr(0, dotPos));
                path = path.substr(dotPos);
                indexPos = path.indexOf('[');
                dotPos = path.indexOf('.');
            }
        }
        res.push(path);
        return res;
    }

    public combinePaths(a: string[], b: string[]): string[] {
        return this.simplifyPath(a.concat(b));
    }

    /**
    * removes `$parent` and `$root` where possible
    */
    public simplifyPath(path: string[]): string[] {
        var ri = path.lastIndexOf("$root");
        if (ri > 0) path = path.slice(ri);
        path = path.filter(v => v != "$data");
        var parIndex = 0;
        while ((parIndex = path.indexOf("$parent")) >= 0) {
            path.splice(parIndex - 1, 2);
        }
        return path;
    }

    private postJSON(url: string, method: string, postData: any, success: (request: XMLHttpRequest) => void, error: (response: XMLHttpRequest) => void) {
        var xhr = XMLHttpRequest ? new XMLHttpRequest() : <XMLHttpRequest>new ActiveXObject("Microsoft.XMLHTTP");
        xhr.open(method, url, true);
        xhr.setRequestHeader("Content-Type", "application/json");
        xhr.onreadystatechange = () => {
            if (xhr.readyState != 4) return;
            if (xhr.status < 400) {
                success(xhr);
            } else {
                error(xhr);
            }
        };
        xhr.send(postData);
    }

    public evaluateOnViewModel(context, expression: string[]) {
        expression.forEach(e => {
            if (e.length == 0 || context == null) return;
            if (ko.isObservable(context)) context = context();
            if (e[0] == "[")
                context = context[eval(e.substring(1, e.length - 1))];
            else if (e[0] == "`")
                context = eval("(function (c) { return c." + e.substring(1, e.length - 1) + "; })")(context);
            else context = context[e];
        });
        return context;
    }
}

// RedwoodEvent is used because CustomEvent is not browser compatible and does not support 
// calling missed events for handler that subscribed too late.
class RedwoodEvent<T extends RedwoodEventArgs> {
    private handlers = [];
    private history = [];

    constructor(public name: string, private triggerMissedEventsOnSubscribe: boolean = false) {
    }

    public subscribe(handler: (data: T) => void) {
        this.handlers.push(handler);

        if (this.triggerMissedEventsOnSubscribe) {
            for (var i = 0; i < this.history.length; i++) {
                handler(history[i]);
                }
            }
        }

    public unsubscribe(handler: (data: T) => void) {
        var index = this.handlers.indexOf(handler);
        if (index >= 0) {
            this.handlers = this.handlers.splice(index, 1);
        }
    }

    public trigger(data: T): void {
        for (var i = 0; i < this.handlers.length; i++) {
            this.handlers[i](data);
            }

        if (this.triggerMissedEventsOnSubscribe) {
            this.history.push(data);
        }
    }
}

class RedwoodEventArgs {
    constructor(public viewModel: any) {
    }
}
class RedwoodErrorEventArgs extends RedwoodEventArgs {
    constructor(public viewModel: any, public xhr: XMLHttpRequest) {
        super(viewModel);
    }
}
class RedwoodBeforePostBackEventArgs extends RedwoodEventArgs {
    public cancel: boolean = false;
    constructor(public sender: HTMLElement, public viewModel: any, public viewModelName: string, public validationTargetPath: string[], public viewModelPath: string[], public command: string) {
        super(viewModel);
    }
}
class RedwoodAfterPostBackEventArgs extends RedwoodEventArgs {
    public isHandled: boolean = false;
    constructor(public sender: HTMLElement, public viewModel: any, public viewModelName: string, public validationTargetPath: string[], public serverResponseObject: any) {
        super(viewModel);
    }
}

var redwood = new Redwood();


// add knockout binding handler for update progress control
ko.bindingHandlers["redwoodUpdateProgressVisible"] = {
    init(element: any, valueAccessor: () => any, allBindingsAccessor: KnockoutAllBindingsAccessor, viewModel: any, bindingContext: KnockoutBindingContext) {
        element.style.display = "none";
        redwood.events.beforePostback.subscribe(e => {
            element.style.display = "";
        });
        redwood.events.afterPostback.subscribe(e => {
            element.style.display = "none";
        });
        redwood.events.error.subscribe(e => {
            element.style.display = "none";
        });
    }
};