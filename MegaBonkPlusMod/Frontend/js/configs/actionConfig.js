export const ACTIONS_CONFIG = [
    {
        id: 'kill_all_enemies',
        name: 'Kill All Enemies',
        modifiers: [
            {
                id: 'kill_enemies_mode_select',
                name: 'Mode',
                type: 'select',
                payloadKey: 'mode',
                defaultValue: 'single',
                options: [
                    { value: 'single', name: 'Single use' },
                    { value: 'toggle', name: 'Toggle' }
                ]
            }
        ]
    },
    {
        id: 'pick_up_all_xp',
        name: 'Pick Up All XP',
        modifiers: [
            {
                id: 'pick_up_xp_mode_select',
                name: 'Mode',
                type: 'select',
                payloadKey: 'mode',
                defaultValue: 'single',
                options: [
                    { value: 'single', name: 'Single use' },
                    { value: 'toggle', name: 'Toggle' }
                ]
            }
        ]
    },
    {
        id: 'add_levels',
        name: 'Add Levels',
        modifiers: [
            {
                id: 'level_amount_input',
                name: 'Amount',
                type: 'number',
                payloadKey: 'amount',
                defaultValue: 1,
                min: 1,
                max: 9999
            }
        ]
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
    },
    {
        id: 'teleport_to_nearest',
        name: 'Teleport to Nearest',
        modifiers: [
            {
                id: 'teleport_to_select',
                name: 'Teleport To',
                type: 'select',
                payloadKey: 'object',
                defaultValue: 'charge_shrine',
                options: [
                    { value: 'charge_shrine', name: 'Charge Shrine' },
                    { value: 'chest', name: 'Chest' },
                    { value: 'boss_spawner', name: 'Boss Spawner' },
                    { value: 'shady_guy', name: 'Shady Guy' },
                    { value: 'microwave', name: 'Microwave' },
                    { value: 'moai_shrine', name: 'Moai Shrine' },
                    { value: 'greed_shrine', name: 'Greed Shrine' },
                    { value: 'challenge_shrine', name: 'Challenge Shrine' },
                    { value: 'magnet_shrine', name: 'Magnet Shrine' },
                    { value: 'cursed_shrine', name: 'Cursed Shrine' },
                    { value: 'open_chest', name: 'Open Chest' }
                ]
            }
        ]
    }
];