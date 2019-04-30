/*
 * Custom block blocks
 * 
 * - Event Hat 
 *   - status_idle
 *   - status_playing
 *   - status_robot_ready (not yet implemented)
 * 
 * - Event Handler
 *   - event_pre_condition
 *     - condition [CONDITION]
 *   - event_handler
 *     - condition [CONDITION]
 *     - action [ACTION]
 * 
 * - Condition => [CONDITION]
 *   - cond_gpio
 *     - gpio_pin
 *       - GPIO_2
 *       - GPIO_3
 *     - gpio_status
 *   - cond_psx_button
 *     - psx_button
 *   - cond_mpu6050
 *     - axis
 *     - axis_check
 *     - axis_value
 *   - cond_touch
 *     - status
 *   - cond_battery_reading
 *     - battery_reading
 *   - cond_batteey_level
 *     - batter_level
 *   - cond_sonic_distance
 *     - devId
 *     - distance_check
 *     - distance_value
 *   - cond_maze
 *     - action
 *     
 * - action => [ACTION]
 *   - action_play_action
 *     - action_id
 *   - action_stop_action
 *   - action_head_led
 *     - led_status
 *   - action_mp3_play_mp3
 *     - mp3_file
 *   - action_mp3_play_file
 *     - mp3_folder
 *     - mp3_file
 *   - action_mp3_stop
 *   - action_sonic
 *     
 *     
*/

// --------------
//   Status Hat
// --------------

Blockly.Blocks['status_idle'] = {
    init: function () {
        this.appendDummyInput()
            .appendField("系统闲置时");
        this.setNextStatement(true, null);
        this.setColour(270);
        this.setTooltip("");
        this.setHelpUrl("");
        this.setMovable(false);
        this.setDeletable(false);
    }
};

Blockly.Blocks['status_playing'] = {
    init: function () {
        this.appendDummyInput()
            .appendField("动作进行中 (注意: 可能会影响当前动作)");
        this.setNextStatement(true, null);
        this.setColour(0);
        this.setTooltip("");
        this.setHelpUrl("");
        this.setMovable(false);
        this.setDeletable(false);
    }
};

Blockly.Blocks['status_robot_ready'] = {
    init: function () {
        this.appendDummyInput()
            .appendField("当机械人启动完成");
        this.setColour(270);
        this.setTooltip("");
        this.setHelpUrl("");
        this.setMovable(false);
        this.setDeletable(false);
    }
};


// -----------------
//   Event Handler
// -----------------
Blockly.Blocks['event_pre_condition'] = {
    init: function () {
        this.appendValueInput("condition")
            .setCheck("CONDITION")
            .appendField("先决条件");
        this.setPreviousStatement(true, null);
        this.setNextStatement(true, null);
        this.setColour(120);
        this.setTooltip("");
        this.setHelpUrl("");
    }
};

Blockly.Blocks['event_handler'] = {
    init: function () {
        this.appendValueInput("condition")
            .setCheck("CONDITION")
            .appendField("当");
        this.appendValueInput("action")
            .setCheck("ACTION")
            .appendField("执行");
        this.setPreviousStatement(true, null);
        this.setNextStatement(true, null);
        this.setColour(135);
        this.setTooltip("");
        this.setHelpUrl("");
    }
};

// ---------
//   Event
// ---------

Blockly.Blocks['cond_gpio'] = {
    init: function () {
        this.appendDummyInput()
            .appendField("GPIO")
            .appendField(new Blockly.FieldDropdown([["13", "13"], ["14", "14"], ["15", "15"], ["16", "16"]]), "gpio_pin")
            .appendField("为")
            .appendField(new Blockly.FieldDropdown([["高电平", "1"], ["低电平", "0"]]), "gpio_status");
        this.setOutput(true, "CONDITION");
        this.setColour(90);
        this.setTooltip("");
        this.setHelpUrl("");
    }
};

Blockly.Blocks['cond_psx_button'] = {
    init: function () {
        this.appendDummyInput()
            .appendField("PSX")
            .appendField(new Blockly.FieldDropdown([["L1", "FFFB"], ["L2", "FFFE"], ["R1", "FFF7"], ["R2", "FFFD"],
            ["Δ", "FFEF"], ["Х", "FFBF"], ["Ο", "FFDF"], ["□", "FF7F"],
            ["▲", "EFFF"], ["▼", "BFFF"], ["◄", "7FFF"], ["►", "DFFF"],
            ["SELECT", "FEFF"], ["START", "F7FF"]]), "psx_button")
            .appendField("按下");
        this.setInputsInline(true);
        this.setOutput(true, "CONDITION");
        this.setColour(90);
        this.setTooltip("");
        this.setHelpUrl("");
    }
};

Blockly.Blocks['cond_mpu6050'] = {
    init: function () {
        this.appendDummyInput()
            .appendField("陀螺仪")
            .appendField(new Blockly.FieldDropdown([["X", "0"], ["Y", "1"], ["Z", "2"]]), "axis")
            .appendField(new Blockly.FieldDropdown([["高於", "2"], ["低於", "3"]]), "axis_check")
            .appendField(new Blockly.FieldNumber(0, -32767, 32767), "axis_value");
        this.setOutput(true, "CONDITION");
        this.setColour(90);
        this.setTooltip("");
        this.setHelpUrl("");
    }
};

Blockly.Blocks['cond_touch'] = {
    init: function () {
        this.appendDummyInput()
            .appendField("头部被")
            .appendField(new Blockly.FieldDropdown([["单拍", "1"], ["双拍", "2"], ["三拍", "3"], ["长按", "255"]]), "touch_status");
        this.setOutput(true, "CONDITION");
        this.setColour(90);
        this.setTooltip("");
        this.setHelpUrl("");
    }
};

Blockly.Blocks['cond_battery_reading'] = {
    init: function () {
        this.appendDummyInput()
            .appendField("电池读数低於")
            .appendField(new Blockly.FieldNumber(0, 0, 9999), "battery_reading");
        this.setOutput(true, "CONDITION");
        this.setColour(90);
        this.setTooltip("");
        this.setHelpUrl("");
    }
};

Blockly.Blocks['cond_battery_level'] = {
    init: function () {
        this.appendDummyInput()
            .appendField("电池馀量低於")
            .appendField(new Blockly.FieldNumber(0, 0, 99), "battery_level")
            .appendField("%");
        this.setOutput(true, "CONDITION");
        this.setColour(90);
        this.setTooltip("");
        this.setHelpUrl("");
    }
};

Blockly.Blocks['cond_sonic_distance'] = {
    init: function () {
        this.appendDummyInput()
            .appendField("超声波测距")
            .appendField(new Blockly.FieldDropdown([["0", "0"], ["1", "1"], ["2", "2"], ["3", "3"]]), "device_id")
            .appendField("距离")
            .appendField(new Blockly.FieldDropdown([["大於", "2"], ["小於", "3"]]), "distance_check")
            .appendField(new Blockly.FieldNumber(0, 0, 10000), "distance_value")
            .appendField("cm");
        this.setOutput(true, "CONDITION");
        this.setColour(90);
        this.setTooltip("");
        this.setHelpUrl("");
    }
};

Blockly.Blocks['cond_maze'] = {
    init: function () {
        this.appendDummyInput()
            .appendField("迷宫导航 指示为")
            .appendField(new Blockly.FieldDropdown([["向前走", "0"], ["向左走", "1"], ["向右走", "2"], ["回头", "3"]]), "action");
        this.setOutput(true, "CONDITION");
        this.setColour(90);
        this.setTooltip("");
        this.setHelpUrl("");
    }
};
// ---------
//   Action
// ---------

Blockly.Blocks['action_play_action'] = {
    init: function () {
        this.appendDummyInput()
            .appendField("播放动作")
            .appendField(new Blockly.FieldNumber(0, 0, 254), "action_id");
        this.setOutput(true, "ACTION");
        this.setColour(0);
        this.setTooltip("");
        this.setHelpUrl("");
    }
};

Blockly.Blocks['action_stop_action'] = {
    init: function () {
        this.appendDummyInput()
            .appendField("停止动作");
        this.setOutput(true, "ACTION");
        this.setColour(0);
        this.setTooltip("");
        this.setHelpUrl("");
    }
};

Blockly.Blocks['action_head_led'] = {
    init: function () {
        this.appendDummyInput()
            .appendField("头灯")
            .appendField(new Blockly.FieldDropdown([["亮着", "1"], ["熄掉", "0"]]), "led_status");
        this.setOutput(true, "ACTION");
        this.setColour(0);
        this.setTooltip("");
        this.setHelpUrl("");
    }
};

Blockly.Blocks['action_mp3_play_mp3'] = {
    init: function () {
        this.appendDummyInput()
            .appendField("播放音乐: MP3")
            .appendField(new Blockly.FieldNumber(0, 0, 9999), "mp3_file");
        this.setOutput(true, "ACTION");
        this.setColour(0);
        this.setTooltip("");
        this.setHelpUrl("");
    }
};

Blockly.Blocks['action_mp3_play_file'] = {
    init: function () {
        this.appendDummyInput()
            .appendField("播放音乐: 目录")
            .appendField(new Blockly.FieldNumber(0, 0, 255), "mp3_folder")
            .appendField(", 档案")
            .appendField(new Blockly.FieldNumber(0, 0, 255), "mp3_file");
        this.setOutput(true, "ACTION");
        this.setColour(0);
        this.setTooltip("");
        this.setHelpUrl("");
    }
};

Blockly.Blocks['action_mp3_stop'] = {
    init: function () {
        this.appendDummyInput()
            .appendField("停止播放音乐");
        this.setOutput(true, "ACTION");
        this.setColour(0);
        this.setTooltip("");
        this.setHelpUrl("");
    }
};

Blockly.Blocks['action_gpio'] = {
    init: function () {
        this.appendDummyInput()
            .appendField("设定 GPIO")
            .appendField(new Blockly.FieldDropdown([["15", "15"]]), "gpio_pin")
            .appendField("为")
            .appendField(new Blockly.FieldDropdown([["高电平", "1"], ["低电平", "0"]]), "gpio_status");
        this.setOutput(true, "ACTION");
        this.setColour(0);
        this.setTooltip("");
        this.setHelpUrl("");
    }
};

Blockly.Blocks['action_system_action'] = {
    init: function () {
        this.appendDummyInput()
            .appendField("系统预设动作")
            .appendField(new Blockly.FieldNumber(0, 1, 255), "system_action_id");
        this.setOutput(true, "ACTION");
        this.setColour(0);
        this.setTooltip("");
        this.setHelpUrl("");
    }
};

Blockly.Blocks['action_servo'] = {
    init: function () {
        this.appendDummyInput()
            .appendField("舵机")
            .appendField(new Blockly.FieldNumber(0, 1, 16), "servo_id")
            .appendField("在")
            .appendField(new Blockly.FieldNumber(0, 50, 250), "action_time")
            .appendField("ms 移动")
            .appendField(new Blockly.FieldNumber(0, -120, 120), "action_angle")
            .appendField("度");
        this.setOutput(true, "ACTION");
        this.setColour(0);
        this.setTooltip("");
        this.setHelpUrl("");
    }
};

Blockly.Blocks['action_sonic'] = {
    init: function () {
        this.appendDummyInput()
            .appendField(new Blockly.FieldDropdown([["啟用", "1"], ["停止", "0"]]), "sonic_status")
            .appendField("超聲波測距");
        this.setOutput(true, "ACTION");
        this.setColour(0);
        this.setTooltip("");
        this.setHelpUrl("");
    }
};