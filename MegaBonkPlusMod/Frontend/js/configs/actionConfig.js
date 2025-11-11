export const ACTIONS_CONFIG = [
    {
        id: 'kill_all_enemies',
        name: 'Kill All Enemies',
        modifiers: []
    },
    {
        id: 'toggle_auto_restart',
        name: 'Toggle Auto-Restarter',
        modifiers: []
    },
    {
        id: 'edit_gold',
        name: 'Give Gold',
        modifiers: [
            {
                id: 'gold_mode_select',
                name: 'Mode',
                type: 'select',
                payloadKey: 'changeMode',
                defaultValue: 'add',
                options: [
                    { value: 'add', name: 'Add Gold' },
                    { value: 'set', name: 'Set Gold' }
                ]
            },
            {
                id: 'gold_amount_input',
                name: 'Amount',
                type: 'number',
                payloadKey: 'amount',
                defaultValue: 1000,
                min: 0
            }
        ]
    },
    {
        id: 'spawn_items',
        name: 'Give Item',
        modifiers: [
            {
                id: 'item_select_dropdown',
                name: 'Item',
                type: 'item-select',
                payloadKey: 'itemId',
                defaultValue: null
            },
            {
                id: 'item_quantity_input',
                name: 'Quantity',
                type: 'number',
                payloadKey: 'quantity',
                defaultValue: 1,
                min: 0,
                max: 99
            }
        ]
    }
];