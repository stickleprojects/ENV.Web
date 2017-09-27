﻿
import { Component, Input, OnChanges, Type } from '@angular/core';
import { Routes } from '@angular/router';



@Component({
    selector: 'data-grid',
    templateUrl: './scripts/app/lib/data-grid.component.html'
})

export class DataGridComponent implements OnChanges {

    // Inspired by  https://medium.com/@ct7/building-a-reusable-table-layout-for-your-angular-2-project-adf6bba3b498

    @Input() records: Iterable<any>;
    @Input() settings = new DataSettings();
    columnMaps: ColumnSettingBase[];
    rowButtons: rowButtonBase[] = [];
    keys: string[] = [];
    private addButton(b: rowButtonBase) {
        if (!b.click)
            b.click = (r) => { };
        if (!b.visible)
            b.visible = r => true;
        this.rowButtons.push(b);
        return b;

    }
    catchErrors(what: any, r: any) {
        what.catch(e => e.json().then(e => {
            console.log(e);
            let s = new ModelState(r);
            r.__modelState = () => s;
            s.message = e.Message;
            s.modelState = e.ModelState;
            this.showError(s.message, s.modelState);

        }));

    }
    private showError(message: string, state: any) {
        if (!message)
            message = "";
        if (state) {
            for (let x in state) {

                let m = x + ": ";
                for (var i = 0; i < state[x].length; i++) {
                    m += state[x][i];
                }
                if (m != message)
                    message += "\n" + m;
            }

        }

        alert(message);
    }

    _getError(col: ColumnSettingBase, r: any) {
        if (r.__modelState) {
            let m = <ModelState<any>>r.__modelState();
            if (m.modelState) {
                let errors = m.modelState[col.key];
                if (errors && errors.length > 0)
                    return errors[0];
            }

        }
        return undefined;
    }
    _colValueChanged(col: ColumnSettingBase, r: any) {
        if (r.__modelState) {
            let m = <ModelState<any>>r.__modelState();
            m.modelState[col.key] = undefined;
        }
    }
    ngOnChanges(): void {
        if (!this.settings)
            return;
        this.rowButtons = [];
        if (this.settings.editable) {

            this.addButton({
                name: "save", click: r => {
                    let s = new ModelState(r);
                    r.__modelState = () => s;
                    if (this.settings.onSavingRow)
                        this.settings.onSavingRow(s);
                    if (s.isValid)
                        this.catchErrors(r.save(), r);
                    else
                        this.showError(s.message, s.modelState);
                }
            });

            this.addButton({
                name: 'Delete', visible: (r) => r.newRow == undefined, click: r => this.catchErrors(r.delete(), r)
            });

        }
        for (let b of this.settings.buttons) {
            this.addButton(b);
        }
        if (!this.records) {
            this.settings.getRecords().then(r => {
                this.records = r;
                if (this.settings.settings.length == 0)
                    this.autoGenerateColumnsBasedOnData();
            });

        }

        if (this.settings.settings.length > 0) { // when settings provided
            this.columnMaps = this.settings.settings;
            this.columnMaps.forEach(s => {
                if (!s.caption)
                    s.caption = makeTitle(s.key);
            });
        } else if (this.records) {
            {
                this.autoGenerateColumnsBasedOnData();

            }
        }
    }
    private autoGenerateColumnsBasedOnData() {
        for (let r of this.records) {
            this.columnMaps = [];
            Object.keys(r).forEach(key => {
                if (typeof (r[key]) != 'function')

                    this.columnMaps.push({
                        key: key,
                        caption: makeTitle(key)
                    });
            });
            break;
        }
    }
    _getRowClass(row: any) {
        if (this.settings.rowClass)
            return this.settings.rowClass(row);
        return "";
    }
    _getColValue(col: ColumnSettingBase, row: any) {
        if (col.getValue)
            return col.getValue(row);
        return row[col.key];
    }
    _getColDataType(col: ColumnSettingBase, row: any) {
        if (col.inputType)
            return col.inputType;
        return "text";
    }
    _getColumnClass(col: ColumnSettingBase, row: any) {

        if (col.cssClass)
            if (isFunction(col.cssClass)) {
                let anyFunc: any = col.cssClass;
                return anyFunc(row);
            }
            else return col.cssClass;
        return '';

    }
    _getEditable(col: ColumnSettingBase) {
        if (!this.settings.editable)
            return false;
        if (!col.key)
            return false
        return !col.readonly;


    }
}
function makeTitle(key: string) {
    return key.slice(0, 1).toUpperCase() + key.replace(/_/g, ' ').slice(1);
}
class DataSettingsBase {
    editable = false;
    settings: ColumnSettingBase[] = [];
    buttons: rowButtonBase[] = [];
    getRecords: () => Promise<Iterable<any>>;
    rowClass?: (row: any) => string;
    onSavingRow?: (s: ModelState<any>) => void;
}
export class DataSettings<rowType> extends DataSettingsBase {
    static getRecords(): any {
        throw new Error("Method not implemented.");
    }

    constructor(settings?: IDataSettings<rowType>) {
        super();
        if (settings) {
            if (settings.columnKeys)
                this.add(...settings.columnKeys);
            if (settings.columnSettings)
                this.add(...settings.columnSettings);

            if (settings.editable)
                this.editable = true;
            if (settings.rowButtons)
                this.buttons = settings.rowButtons;
            if (settings.restUrl) {
                this.restList = new RestList<rowType>(settings.restUrl);
            } if (settings.rowCssClass)
                this.rowClass = settings.rowCssClass;
            if (settings.onSavingRow)
                this.onSavingRow = settings.onSavingRow;
            this.getOptions = settings.get;
        }

    }

    get(options: getOptions<rowType>) {
        this.restList.get(options);
    }
    private getOptions: getOptions<rowType>;
    getRecords: () => Promise<Iterable<any>> = () => this.restList.get(this.getOptions).then(() => this.restList);

    restList: RestList<rowType>;
    get items(): rowType[] {
        if (this.restList)
            return this.restList.items;
        return undefined;
    }

    private settingsByKey = {};

    add(...columns: ColumnSetting<rowType>[]);
    add(...columns: string[]);
    add(...columns: any[]) {
        for (let c of columns) {
            let s: ColumnSetting<rowType>;
            let x = c as ColumnSetting<rowType>;
            if (x.key || x.getValue) {
                s = x;
            }
            else {
                s = { key: c };
            }
            if (s.key) {
                let existing: ColumnSetting<rowType> = this.settingsByKey[s.key];
                if (existing) {
                    if (s.caption)
                        existing.caption = s.caption;
                    if (s.cssClass)
                        existing.cssClass = s.cssClass;
                    if (s.getValue)
                        existing.getValue = s.getValue;
                    if (s.readonly)
                        existing.readonly = s.readonly;

                }
                else {
                    this.settings.push(s);
                    this.settingsByKey[s.key] = s;
                }

            }
            else
                this.settings.push(s);


        }
    }

}
interface IDataSettings<rowType> {
    editable?: boolean,
    columnSettings?: ColumnSetting<rowType>[],
    columnKeys?: string[],
    restUrl?: string,
    rowCssClass?: (row: rowType) => string;
    rowButtons?: rowButton<rowType>[],
    get?: getOptions<rowType>,
    onSavingRow?: (s: ModelState<rowType>) => void;
}
class ModelState<rowType> {
    row: rowType;
    constructor(private _row: any) {
        this.row = _row;
    }

    isValid = true;
    message: string;
    addError(key: string, message: string) {
        this.isValid = false;
        let current = this.modelState[key];
        if (!current) {
            current = this.modelState[key] = [];
        }
        current.push(message);
    }
    required(key: string, message = 'Required') {
        let value = this._row[key];
        if (value == undefined || value == null || value == "" || value == 0)
            this.addError(key, message);
    }
    addErrorMessage(message: string) {
        this.isValid = false;
        this.message = message;
    }
    modelState = {};
}

interface ColumnSettingBase {
    key?: string;
    caption?: string;
    readonly?: boolean;
    getValue?: (row: any) => any;
    cssClass?: (string | ((row: any) => string));
    inputType?: string;
}
interface ColumnSetting<rowType> extends ColumnSettingBase {
    getValue?: (row: rowType) => any;
    cssClass?: (string | ((row: rowType) => string));
}
interface rowButtonBase {

    name?: string;
    visible?: (r: any) => boolean;
    click?: (r: any) => void;

}
interface rowButton<rowType> extends rowButtonBase {
    visible?: (r: rowType) => boolean;
    click?: (r: rowType) => void;

}
function isFunction(functionToCheck) {
    var getType = {};
    return functionToCheck && getType.toString.call(functionToCheck) === '[object Function]';
}


export class RestList<T extends hasId> implements Iterable<T>{
    [Symbol.iterator](): Iterator<T> {
        return this.items[Symbol.iterator]();
    }


    items: (restListItem & T)[] = [];
    constructor(private url: string) {

    }
    private map(item: T): restListItem & T {

        let x = <any>item;
        let id = x.id;
        x.save = () => this.save(id, x);
        x.delete = () => {
            return fetch(this.url + '/' + id, { method: 'delete' }).then(onSuccess, onError).then(() => {
                this.items.splice(this.items.indexOf(x), 1);
            });

        }
        return <restListItem & T>x;
    }

    get(options?: getOptions<T>) {

        let url = new urlBuilder(this.url);
        if (options) {
            url.addObject({
                _limit: options.limit,
                _page: options.page,
                _sort: options.orderBy,
                _order: options.orderByDir
            });
            url.addObject(options.isEqualTo);
            url.addObject(options.isGreaterOrEqualTo, "_gte");
            url.addObject(options.isLessOrEqualTo, "_lte");
            url.addObject(options.isGreaterThan, "_gt");
            url.addObject(options.isLessThan, "_lt");
            url.addObject(options.isDifferentFrom, "_ne");
        }


        return myFetch(url.url).then(r => {
            let x: T[] = r;
            this.items = r.map(x => this.map(x));
        });
    }
    add(): T {
        let x: newItemInList = { newRow: true };
        this.items.push(this.map(x as any as T));
        return x as any as T;
    }
    private save(id: any, c: restListItem & T) {

        let nr: newItemInList = c as any as newItemInList;
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

            return myFetch(this.url + '/' + id, {
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
class urlBuilder {
    constructor(public url: string) {
    }
    add(key: string, value: any) {
        if (value == undefined)
            return;
        if (this.url.indexOf('?') >= 0)
            this.url += '&';
        else
            this.url += '?';
        this.url += encodeURIComponent(key) + '=' + encodeURIComponent(value);
    }
    addObject(object: any, suffix = '') {
        if (object != undefined)
            for (var key in object) {
                this.add(key + suffix, object[key]);
            }
    }
}
function myFetch(url: string, init?: RequestInit): Promise<any> {

    return fetch(url, init).then(onSuccess, error => {

    });

}
function onSuccess(response: Response) {

    if (response.status >= 200 && response.status < 300)
        return response.json();
    else throw response;

}
function onError(error: any) {
    throw error;
}
interface newItemInList {
    newRow: boolean;
}
interface hasId {
    id?: any;
}
interface restListItem {
    save: () => void;
    delete: () => void;
}
export interface getOptions<T> {
    isEqualTo?: T;
    isGreaterOrEqualTo?: T;
    isLessOrEqualTo?: T;
    orderBy?: string;
    orderByDir?: string;
    page?: number;
    limit?: number;
    isGreaterThan?: T;
    isLessThan?: T;
    isDifferentFrom?: T;

}

export class Lookup<lookupType, idType_or_MainTableType> {

    constructor(url: string, private options?: (mt: idType_or_MainTableType, o: getOptions<lookupType>) => void) {
        if (!options) {
            this.options = (mt, o) => o.isEqualTo = <lookupType><any>{ id: mt };
        }
        this.categories = new RestList<lookupType>(url);
    }

    categories: RestList<lookupType>;
    private cache: {};

    get(r: any): lookupType {
        return this.getInternal(r).value;
    }
    found(r: any): boolean {
        return this.getInternal(r).found;
    }

    private getInternal(r: any): lookupRowInfo<lookupType> {

        let find: getOptions<lookupType> = {};
        this.options(<idType_or_MainTableType>r, find);
        let key = JSON.stringify(find);
        if (this.cache == undefined)
            this.cache = {};
        if (this.cache[key]) {
            return this.cache[key];
        } else {
            let res = new lookupRowInfo<lookupType>();
            this.cache[key] = res;
            this.categories.get(find).then(() => {
                res.loading = false;
                if (this.categories.items.length > 0) {
                    res.value = this.categories.items[0];
                    res.found = true;
                }
            });
            return res;
        }

    }
}

class lookupRowInfo<type> {
    found = false;
    loading = true;
    value: type = {} as type;

}
export class AppHelper {
    constructor() {
        
    }
    Routes: Routes =
    [
    ];
    menues: MenuEntry[]=[];

    Components: Type<any>[] = [DataGridComponent];

    Register(component: Type<any>) {
        this.Components.push(component);
        let name = component.name;
        if (this.Routes.length == 0)
            this.Routes.push({ path: '', redirectTo: '/' + name, pathMatch: 'full' });
        this.Routes.splice(0, 0, { path: name, component: component });
        this.menues.push({
            path: '/' + name,
            text:name
        });
    }
    Add(c: Type<any>) {
        this.Components.push(c);
    }

}
interface MenuEntry {
    path: string,
    text:string
}



