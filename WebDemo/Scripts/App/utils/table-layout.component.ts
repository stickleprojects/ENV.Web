﻿// https://medium.com/@ct7/building-a-reusable-table-layout-for-your-angular-2-project-adf6bba3b498
import { Component, Input, OnChanges } from '@angular/core';

@Component({
    selector: 'ct-table',
    templateUrl: './scripts/app/utils/table-layout.component.html'
})
export class TableLayoutComponent implements OnChanges {


    @Input() records: Iterable<any>;
    @Input() settings = new TableSettings();
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
    ngOnChanges(): void {

        this.rowButtons = [];
        if (this.settings.editable) {

            this.addButton({ name: "save", click: r => r.save() });

            this.addButton({
                name: 'Delete', visible: (r) => r.newRow == undefined, click: r => r.delete()
            });

        }
        for (let b of this.settings.buttons) {
            this.addButton(b);
        }


        if (this.settings.settings.length > 0) { // when settings provided
            this.columnMaps = this.settings.settings;
            this.columnMaps.forEach(s => {
                if (!s.caption)
                    s.caption = makeTitle(s.key);
            });
        } else {
            {
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
        }
    }
    _getRowClass(row: any) {
        return "";
    }
    _getColValue(col: ColumnSettingBase, row: any) {
        if (col.getValue)
            return col.getValue(row);
        return row[col.key];
    }
    _getColumnClass(col: ColumnSettingBase, row: any) {
        if (col.columnClass)
            return col.columnClass(row);
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
class TableSettingsBase {
    editable = false;
    settings: ColumnSettingBase[] = [];
    buttons: rowButtonBase[] = [];
}
export class TableSettings<rowType> extends TableSettingsBase {

    constructor(settings?: TableSettingsInterface<rowType>) {
        super();
        if (settings) {
            if (settings.columnSettings)
                this.add(...settings.columnSettings);
            else if (settings.columnKeys)
                this.add(...settings.columnKeys);
            if (settings.editable)
                this.editable = true;
            if (settings.rowButtons)
                this.buttons = settings.rowButtons;
        }

    }


    add(...columns: ColumnSetting<rowType>[]);
    add(...columns: string[]);
    add(...columns: any[]) {
        for (let c of columns) {
            let x = c as ColumnSetting<rowType>;
            if (x.key || x.getValue)
                this.settings.push(x);
            else this.settings.push({ key: c });
        }
    }

}
interface TableSettingsInterface<rowType> {
    editable?: boolean,
    columnSettings?: ColumnSetting<rowType>[],
    columnKeys?: string[],
    rowClass?: (row: any) => string;
    rowButtons?: rowButton<rowType>[]
}

interface ColumnSettingBase {
    key?: string;
    caption?: string;
    readonly?: boolean;
    getValue?: (row: any) => any;
    columnClass?: (row: any) => string;
}
interface ColumnSetting<rowType> extends ColumnSettingBase {
    getValue?: (row: rowType) => any;
    columnClass?: (row: rowType) => string;
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