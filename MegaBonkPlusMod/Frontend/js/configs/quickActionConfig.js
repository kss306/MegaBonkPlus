export const QUICK_ACTIONS_CONFIG = [
    {
        id: 'quick_kill_all',
        label: 'KILL ALL',
        type: 'split-button',
        actionId: 'kill_all_enemies',
        menuItems: [
            {
                id: 'kill_all_looping_checkbox',
                label: 'Looping',
                type: 'checkbox',
                payloadKey: 'looping'
            }
        ]
    },
    {
        id: 'quick_pick_all_xp',
        label: 'PICKUP XP',
        type: 'split-button',
        actionId: 'pick_up_all_xp',
        menuItems: [
            {
                id: 'pick_up_all_xp_looping_checkbox',
                label: 'Looping',
                type: 'checkbox',
                payloadKey: 'looping'
            }
        ]
    },
    {
        id: 'quick_add_10000_gold',
        label: '10.000 GOLD',
        type: 'simple-button',
        actionId: 'edit_gold',
        payload: {
            changeMode: 'add',
            amount: 10000
        }
    },
    {
        id: 'quick_add_5_Levels',
        label: '5 LEVELS',
        type: 'simple-button',
        actionId: 'add_levels',
        payload: {
            amount: 5
        }
    },
];