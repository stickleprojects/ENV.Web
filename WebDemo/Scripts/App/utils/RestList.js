"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
class RestList {
    constructor(url) {
        this.url = url;
        this.items = [];
    }
    [Symbol.iterator]() {
        return this.items[Symbol.iterator]();
    }
    map(item) {
        let x = item;
        let id = x.id;
        x.save = () => this.save(id, x);
        x.delete = () => {
            return fetch(this.url + '/' + id, { method: 'delete' }).then(() => {
                this.items.splice(this.items.indexOf(x), 1);
            });
        };
        return x;
    }
    get(options) {
        let url = new urlBuilder(this.url);
        if (options) {
            url.addObject({
                _limit: options.limit,
                _page: options.page,
                _sort: options.sort,
                _order: options.order
            });
            url.addObject(options.isEqualTo);
            url.addObject(options.isGreaterOrEqualTo, "_gte");
            url.addObject(options.isLessOrEqualTo, "_lte");
            url.addObject(options.isGreaterThan, "_gt");
            url.addObject(options.isLessThan, "_lt");
            url.addObject(options.isDifferentFrom, "_ne");
        }
        return myFetch(url.url).then(r => {
            let x = r;
            this.items = r.map(x => this.map(x));
        });
    }
    add() {
        let x = { newRow: true };
        this.items.push(this.map(x));
        return x;
    }
    save(id, c) {
        let nr = c;
        if (nr.newRow)
            return myFetch(this.url, {
                method: 'post',
                headers: {
                    'Content-type': "application/json"
                },
                body: JSON.stringify(c)
            }).then(response => {
                this.items[this.items.indexOf(c)] = this.map(response);
            });
        else {
            return myFetch(this.url + '/' + c.id, {
                method: 'put',
                headers: {
                    'Content-type': "application/json"
                },
                body: JSON.stringify(c)
            }).then(response => {
                this.items[this.items.indexOf(c)] = this.map(response);
            });
        }
    }
}
exports.RestList = RestList;
class urlBuilder {
    constructor(url) {
        this.url = url;
    }
    add(key, value) {
        if (value == undefined)
            return;
        if (this.url.indexOf('?') >= 0)
            this.url += '&';
        else
            this.url += '?';
        this.url += encodeURIComponent(key) + '=' + encodeURIComponent(value);
    }
    addObject(object, suffix = '') {
        if (object != undefined)
            for (var key in object) {
                this.add(key + suffix, object[key]);
            }
    }
}
function myFetch(url, init) {
    return fetch(url, init).then(onSuccess, error => {
    });
    function onSuccess(response) {
        if (response.status >= 200 && response.status < 300)
            return response.json();
        else
            throw response;
    }
    function onError(error) {
        throw error;
    }
}
class Lookup {
    constructor(url, options) {
        this.options = options;
        this.categories = new RestList(url);
    }
    get(r) {
        let find = {};
        this.options(r, find);
        let key = JSON.stringify(find);
        if (this.cache == undefined)
            this.cache = {};
        if (this.cache[key]) {
            return this.cache[key];
        }
        else {
            let res = {};
            this.cache[key] = res;
            this.categories.get(find).then(() => {
                if (this.categories.items.length > 0)
                    this.cache[key] = this.categories.items[0];
            });
            return res;
        }
    }
}
exports.Lookup = Lookup;
//# sourceMappingURL=RestList.js.map