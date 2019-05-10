// Source: https://atech.blog/atech/creating-a-simple-custom-event-system-in-javascript
// No error handling;
// No argument checking;
class Dispatcher {
    constructor() {
        this.events = {};
    }

    dispatch(eventName, data) {
        // TODO: dispatch the provided event
        // First we grab the event
        const event = this.events[eventName];
        // If the event exists then we fire it!
        if (event) {
            event.fire(data);
        }
    }

    on(eventName, callback) {
        // TODO: add the event listener to the provided event
        // First we grab the event from this.events
        let event = this.events[eventName];
        // If the event does not exist then we should create it!
        if (!event) {
            event = new DispatcherEvent(eventName);
            this.events[eventName] = event;
        }
        // Now we add the callback to the event
        event.registerCallback(callback);
    }

    off(eventName, callback) {
        // TODO: remove the event listener from the provided event
        // First get the correct event
        const event = this.events[eventName];
        // Check that the event exists and it has the callback registered
        if (event && event.callbacks.indexOf(callback) > -1) {
            // if it is registered then unregister it!
            event.unregisterCallback(callback);
            // if the event has no callbacks left, delete the event
            if (event.callbacks.length === 0) {
                delete this.events[eventName];
            }
        }
    }
}

class DispatcherEvent {
    constructor(eventName) {
        this.eventName = eventName;
        this.callbacks = [];
    }

    registerCallback(callback) {
        // TODO: Add the provided callback to the event
        this.callbacks.push(callback);
    }

    unregisterCallback(callback) {
        // TODO: Remove the provided callback from the event
        // Get the index of the callback in the callbacks array
        const index = this.callbacks.indexOf(callback);
        // If the callback is in the array then remove it
        if (index > -1) {
            this.callbacks.splice(index, 1);
        }
    }

    fire(data) {
        // TODO: Call each callback with the provided data
        // We loop over a cloned version of the callbacks array
        // in case the original array is spliced while looping
        const callbacks = this.callbacks.slice(0);
        // loop through the callbacks and call each one
        callbacks.forEach((callback) => {
            callback(data);
        });
    }
}