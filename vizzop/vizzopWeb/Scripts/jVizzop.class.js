// jVizzop Object Oriented zs_Class
//
// Version 1.0.2
//
// Create By Hassan Jodat Shandi
// http://doob.ir/
// 27 Feb 2010
//

jVizzop.extend({
    // define zs_Class core object
    zs_Class: function () {
    },
    // define interface core object
    Interface: function () {
    }
});

jVizzop.extend(jVizzop.zs_Class, {
    extend: function () {
        var options, name, src, copy, copyIsArray, clone,
        skip = arguments[0],
		target = arguments[1] || {},
		i = 2,
		length = arguments.length,
		deep = false;

        // Handle a deep copy situation
        if (typeof target === 'boolean') {
            deep = target;
            target = arguments[2] || {};
            // skip the boolean and the target
            i = 3;
        }

        // Handle case when target is a string or something (possible in deep copy)
        if (typeof target !== 'object' && !jVizzop.isFunction(target)) {
            target = {};
        }

        // extend jVizzop itself if only one argument is passed
        if (length === i) {
            target = this;
            --i;
        }

        for (; i < length; i++) {
            // Only deal with non-null/undefined values
            if ((options = arguments[i]) != null) {
                // Extend the base object
                for (name in options) {
                    if (name != skip) {
                        src = target[name];
                        copy = options[name];

                        // Prevent never-ending loop
                        if (target === copy) {
                            continue;
                        }

                        // Recurse if we're merging plain objects or arrays
                        if (deep && copy && (jVizzop.isPlainObject(copy) || (copyIsArray = jVizzop.isArray(copy)))) {
                            if (copyIsArray) {
                                copyIsArray = false;
                                clone = src && jVizzop.isArray(src) ? src : [];

                            } else {
                                clone = src && jVizzop.isPlainObject(src) ? src : {};
                            }

                            // Never move original objects, clone them
                            target[name] = jVizzop.extend(deep, clone, copy);

                            // Don't bring in undefined values
                        } else if (copy !== undefined) {
                            target[name] = copy;
                        }
                    }
                }
            }
        }

        // Return the modified object
        return target;
    },
    getFunctionBody: function (func) {
        // Get content between first { and last }
        var m = func.toString().match(/\{([\s\S]*)\}/m)[1];
        // Strip comments
        return jVizzop.trim(m.replace(/^\s*\/\/.*$/mg, ''));
    },
    Function: function () { }
});

// define Function body
jVizzop.extend(jVizzop.zs_Class.Function, {
    Empty: function () { }
});

// define interface elements
jVizzop.extend(jVizzop.Interface.prototype, {
    attributes: [],
    properties: [],
    methods: []
});

// define zs_Class elements
jVizzop.extend(jVizzop.zs_Class, {
    // this method create getter and setter property
    createGetSet: function (object, name, element) {
        eval('object.prototype["get' + name + '"] = function () { return this.' + element + '; };');
        eval('object.prototype["set' + name + '"] = function () { this.' + element + ' = arguments[0]; };');
        if (!object.prototype[element]) {
            object.prototype[element] = '';
        }
    },

    // this method create getter and setter property in runtime
    createGetSetRuntime: function (object, name, element) {
        eval('object.constructor.prototype["get' + name + '"] = function () { return this.' + element + '; };');
        eval('object.constructor.prototype["set' + name + '"] = function () { this.' + element + ' = arguments[0]; };');
        if (!object.constructor.prototype[element]) {
            object.constructor.prototype[element] = '';
        }
    },

    // this method create module in window object
    createNamespace: function (name) {
        var names = name.split('.'),
            namespaceStr = 'window';
        while (names.length) {
            var name = names.shift();
            namespaceStr += '["' + name + '"]';
        }
        return namespaceStr;
    },

    // use this method for understanding interface elements is implemented in zs_Class
    isIn: function (object, name) {
        for (var item in object.prototype) {
            if (item == name) {
                if (jVizzop.isArray(object.prototype[item]))
                    return 0;
                else if (jVizzop.isFunction(object.prototype[item]))
                    return 1;
                else if (jVizzop.isEmptyObject(object.prototype[item]) || jVizzop.isPlainObject(object.prototype[item]))
                    return 2;
                else if (jVizzop.isXMLDoc(object.prototype[item]))
                    return 3;
                else
                    return 0;
            }
        }
        return -1;
    },

    // this method for create zs_Class object
    create: function () {
        var parent = null,
            elements = null,
            base = null,
            options = {
                abstract: false,
                getset: [],
                implements: [],
                module: ''
            };

        // check for extending
        if (jVizzop.isFunction(arguments[0])) {
            parent = arguments[0];
            elements = arguments[1];
            if (arguments[2]) {
                jVizzop.extend(options, arguments[2] || {});
            }
        }
        else {
            elements = arguments[0];
            if (arguments[1]) {
                jVizzop.extend(options, arguments[1] || {});
            }
        }

        // create new zs_Class core
        function handle() {
            // check if zs_Class is abstracted
            if (this.options.abstract) {
                throw new Error('abstract zs_Classes cannot be instantiated');
            }

            // execute constructor method
            try {
                this.initialize.apply(this, arguments);
            } catch (ex) { }
        }

        // extend zs_Class base methods in new zs_Class core
        jVizzop.extend(handle.prototype, jVizzop.zs_Class);

        // extend parent zs_Class methods in new zs_Class core
        if (parent) {
            // extend parent zs_Class methods in new zs_Class core
            function clone(obj) {
                if (obj == null || typeof (obj) != 'object')
                    return obj;

                var temp = obj.constructor(); // changed
                if (temp == undefined)
                    temp = {};
                for (var key in obj)
                    temp[key] = clone(obj[key]);
                return temp;
            }
            for (property in parent.prototype) {
                if (property != 'superClass') {
                    switch (jVizzop.type(parent.prototype[property])) {
                        case 'function':
                            try {
                                //console.log(parent);
                                //console.log(property);
                                eval('handle.prototype[property] = ' + parent.prototype[property]);
                            } catch (err) { }
                            break;
                        case 'object':
                            try {
                                handle.prototype[property] = clone(parent.prototype[property]);
                            } catch (err) { }
                            break;
                        default:
                            try {
                                handle.prototype[property] = parent.prototype[property];
                            } catch (err) { }
                            break;
                    }
                }
            }
            //jVizzop.zs_Class.extend('superClass', true, handle.prototype, parent.prototype);
            // save parent
            handle.prototype.superClass = parent.prototype;
        }

        // extend user defined methods in new zs_Class core
        jVizzop.extend(handle.prototype, elements || {});
        handle.prototype.options = options;

        // define getter and setter functions
        if (options.getset.length > 0) {
            for (var i = 0; i < options.getset.length; i++) {
                var name = options.getset[i][0],
                    element = options.getset[i][1];
                this.createGetSet(handle, name, element);
            }
        }

        // check for impelemented elements from interface
        if (options.implements.length > 0) {
            var attributesMustImplemented = [],
                propertiesMustImplemented = [],
                methodsMustImplemented = [];

            // extract elements from interface
            for (var i = 0; i < options.implements.length; i++) {
                jVizzop.merge(attributesMustImplemented, options.implements[i].attributes);
                jVizzop.merge(propertiesMustImplemented, options.implements[i].properties);
                jVizzop.merge(methodsMustImplemented, options.implements[i].methods);
            }

            var didNotImplemented = false,
                msg = 'must be implemented';

            // check for attributes    
            for (var i = 0; i < attributesMustImplemented.length; i++) {
                var result = this.isIn(handle, attributesMustImplemented[i]);
                if (result != 0 && result != 2) {
                    didNotImplemented = true;
                    msg = 'attribute: ' + attributesMustImplemented[i] + ', ' + msg;
                }
            }

            // check for properties
            for (var i = 0; i < propertiesMustImplemented.length; i++) {
                var resultGet = this.isIn(handle, 'get' + propertiesMustImplemented[i]),
                    resultSet = this.isIn(handle, 'set' + propertiesMustImplemented[i]);
                if (resultGet != 1) {
                    didNotImplemented = true;
                    msg = 'property: get' + propertiesMustImplemented[i] + ', ' + msg;
                }
                else if (resultSet != 1) {
                    didNotImplemented = true;
                    msg = 'property: set' + propertiesMustImplemented[i] + ', ' + msg;
                }
            }

            // check for methods
            for (var i = 0; i < methodsMustImplemented.length; i++) {
                var result = this.isIn(handle, methodsMustImplemented[i]);
                if (result != 1) {
                    didNotImplemented = true;
                    msg = 'method: ' + methodsMustImplemented[i] + ', ' + msg;
                }
            }

            if (didNotImplemented) {
                throw new Error(msg);
            }
        }

        // check if zs_Class is module type, create module
        if (options.module != '') {
            var names = options.module.split('.'),
                name = names[0];
            if (window[name] == null || window[name] == undefined)
                window[name] = new function () { };
            for (var i = 1; i < names.length; i++) {
                name += '.' + names[i];
                eval('if (' + this.createNamespace(name) + ' == null || ' + this.createNamespace(name) + ' == undefined) ' + this.createNamespace(name) + ' = new function() {};');
            }
            eval('jVizzop.extend(' + this.createNamespace(name) + ', handle.prototype);');
        }

        return handle;
    },

    // for add method to zs_Class in runtime
    addMethods: function () {
        if (arguments[0]) {
            jVizzop.extend(this.constructor.prototype, arguments[0]);
        }
    },

    // for add attribute to zs_Class in runtime
    addAttributes: function () {
        if (arguments[0]) {
            jVizzop.extend(this.constructor.prototype, arguments[0]);
        }
    },

    // for add property to zs_Class in runtime
    addProperty: function () {
        try {
            var name = arguments[0],
				element = arguments[1];
            this.createGetSetRuntime(this, name, element);
        } catch (ex) { }
    },

    // this method is use to get value And set value of property
    property: function () {
        // get value section
        if (arguments.length == 1 && this.constructor.prototype.hasOwnProperty('get' + arguments[0])) {
            if (arguments[0]) {
                if (jVizzop.isFunction(this['get' + arguments[0]])) {
                    return this['get' + arguments[0]]();
                }
            }
        }

            //set value section
        else if (this.constructor.prototype.hasOwnProperty('set' + arguments[0])) {
            if (jVizzop.isFunction(this['set' + arguments[0]])) {
                this['set' + arguments[0]](arguments[1]);
                return this['get' + arguments[0]]();
            }
        }
    },

    // check if two zs_Class is equal
    equal: function () {
        if (arguments[1]) {
            return arguments[0].constructor.prototype == arguments[1].constructor.prototype;
        }
        else {
            return this.constructor.prototype == arguments[0].constructor.prototype;
        }
    },

    // create fresh clone object from zs_Class object
    clone: function () {
        function handle() {
            try {
                this.initialize.apply(this, arguments);
            } catch (ex) { }
        }
        if (arguments[0] == true) {
            jVizzop.extend(handle.prototype, this.constructor.prototype);
            return handle;
        }
        else {
            jVizzop.extend(handle.constructor.prototype, this.constructor.prototype);
            return handle;
        }
    },

    toString: function () {
        //return 'Design By Hassan Jodat Shandi';
        return 'ToString Not Implemented';
    },

    base: function () {
        try {
            var methodName = arguments[0];
            this['temp' + methodName] = this.superClass[methodName];
            var temp = this.superClass;
            if (this.superClass.superClass) {
                this.superClass = this.superClass.superClass;
            }
            var caller = 'this["temp" + methodName](';
            if (arguments[1])
                caller += 'arguments[1]';
            for (var i = 2; i < arguments.length; i++)
                caller += ', arguments[' + i + ']';
            caller += ');';
            eval(caller);
            this['temp' + methodName] = null;
            this.superClass = temp;
        } catch (ex) { }
    }
});
