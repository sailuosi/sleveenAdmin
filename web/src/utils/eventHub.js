//超简易的事件插座


export function createEventHub() {
    return {
        events: [],
        tails: {},
        register: function (eventName) {
            if (this.events.indexOf(eventName) != -1) return;
            this.events.push(eventName);
            this.tails[eventName] = [];
        },
        on(eventName, cb) {
            if (this.tails[eventName] == void 0) {
                this.register(eventName);
            }
            if (typeof cb != "function") return;
            this.tails[eventName].push(cb);
        },
        fire(eventName, info) {
            if (this.events.indexOf(eventName) == -1) this.register(eventName);
            this.tails[eventName].forEach((tar, t) => {
                tar(info);
            });
        },
        has(eventName){
            if(this.events.indexOf(eventName)==-1){
                return false;
            }
            if(this.tails[eventName].length==0){
                return false;
            }
            return true;
        },
    };
}