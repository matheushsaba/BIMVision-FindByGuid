//this file is a part of BIM Vision Plugin SDK

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RGiesecke.DllExport;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using System.Reflection;



// API for .NET, add it to your project
// You need install NuGet package: "Unmanaget Exports (DllExport for .Net)"
//
// use "Plugin" and "ApiWrapper" classes
// for REST and DETAILED DOCUMENTATION look at the comments in "ifc_plugin_api.h" form C/C++ SDK
// function names and behavior are sililar as in API for C/C++ SDK
//
namespace BIMVision
{
    /// <summary>
    /// version of SDK, do not change
    /// </summary>
    public struct SDK
    {
        public const int API_VERSION_MAJOR = 5;
        public const int API_VERSION_MINOR = 50;
    }

    /// <summary>
    /// Plugin main class
    /// </summary>
    #region Plugin

    [StructLayout(LayoutKind.Sequential)]
    public struct ApiVersion
    {
        public int major;
        public int minor;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct PluginInfo
    {
        /// <summary>
        /// plugin name, one line of text
        /// </summary>
        [MarshalAs(UnmanagedType.LPWStr)]
        public string name;

        /// <summary>
        /// producer/company name, one line of text
        /// </summary>
        [MarshalAs(UnmanagedType.LPWStr)]
        public string producer;

        /// <summary>
        /// adress of producer/plugin site, one line of text
        /// </summary>
        [MarshalAs(UnmanagedType.LPWStr)]
        public string www;

        /// <summary>
        /// email of producer/plugin contact, one line of text
        /// </summary>
        [MarshalAs(UnmanagedType.LPWStr)]
        public string email;

        /// <summary>
        /// default short plugin description, can contain multiple lines
        /// </summary>
        [MarshalAs(UnmanagedType.LPWStr)]
        public string description;

        /// <summary>
        /// directory where are help files and description file
        /// </summary>
        [MarshalAs(UnmanagedType.LPWStr)]
        public string help_directory;

        /// <summary>
        /// link to plugin actualization info
        /// </summary>
        [MarshalAs(UnmanagedType.LPWStr)]
        public string update_url;
    }

    /// <summary>
    /// plugin identifiter
    /// </summary>
    public struct PLUGIN_ID
    {
        public Int32 id;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TabSheetPlacement
    {
        public int id;         // tab id
        public IntPtr handle;  // tab sheet window handle
        public int width;      // tab sheet width
        public int height;     // tab sheet height
    };

    public enum TabSheetState
    {
        ts_activate,    // tab sheet activated
        ts_deactivate,  // tab sheet deactivated
        ts_close,       // tab sheet closed
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct TabSheetChange
    {
        public int id;
        public int state;
    };


    public enum DropEffect : int
    {
        None = 0,
        Move = 1,
        Copy = 2
    }

    /// <summary>
    /// inherit from this class to make your plugin for BIM Vision
    /// </summary>
    public abstract class Plugin
    {
        /// <summary>
        /// returns directory where is plugin dll, You can mage subdirectory named same as plugin dll
        /// and and place Your plugin data there (beter to use "%programdata%/plugin_dll_name")
        /// </summary>
        /// <returns></returns>
        public string GetPluginPath()
        {
            return Path.GetDirectoryName(typeof(Plugin).Assembly.Location);
        }

        /// <summary>
        /// first called (and never changed) method of plugin, gets version of api with
        ///  which plugin was compiled (and also passes version of viewer api)
        /// </summary>
        /// <param name="viewer">version of viewer api</param>
        /// <param name="plugin">version of api with which plugin was compiled, put there API_VERSION_MAJOR and API_VERSION_MINOR defines from this file</param>
        public virtual void GetApiVersion(ApiVersion viewer, ref ApiVersion plugin)
        {
            plugin.major = SDK.API_VERSION_MAJOR;
            plugin.minor = SDK.API_VERSION_MINOR;

            if (viewer.major != SDK.API_VERSION_MAJOR || viewer.minor < SDK.API_VERSION_MINOR)
            {
                //plugin will be not loaded
            }
        }

        /// <summary>
        /// second called (and never changed) method of plugin
        /// called once in the begining after 'GetApiVersion()'    
        /// plugin version should be in DLL Version Information (resources)
        /// </summary>
        /// <param name="info">plugin info texts for plugin manager</param>
        public abstract void GetPluginInfo(ref PluginInfo info);

        /// <summary>
        ///  always called once in the begining
        ///  if You return NULL or invalid key plugin will be in unregistered mode
        ///  if some other plugin has same key this plugin wont be loaded
        /// </summary>
        /// <returns>plugin key</returns>
        public abstract byte[] GetPluginKey();

        /// <summary>
        /// plugin initialization, from here plugin may refer to the viewer api
        /// </summary>
        /// <param name="pid">identifiter of plugin given by the viewer, plugin must pass it in all api call</param>
        /// <param name="registered">
        /// if is false - plugin is in unregistered mode (invalid or empty plugin key or key is not activated)
        /// </param>
        /// <param name="viewerHwnd">
        /// You can use it in messagebox (will be modal) and in creation of
        ///  your plugin gui windows (ToolWindow) (they will cooperate with viewer; minimize, restowe, hide, stay on top etc...)
        ///  </param>
        public abstract void OnLoad(PLUGIN_ID pid, bool registered, IntPtr viewerHwnd);

        /// <summary>
        /// if plugin is in demo mode and contain this function it will be called in 100st,
        /// You shuld end Your demo here / prompt user / etc...
        /// </summary>
        public abstract void OnCallLimit();

        /// <summary>
        /// after leaving this function plugin already can not use the api
        /// GUI elements and events are automatically removed by viewer
        /// </summary>
        public virtual void OnUnload() { }

        /// <summary>
        /// called after user changes color scheme in viewer
        /// </summary>
        public virtual void OnGuiColorsChange() { }


        public virtual DropEffect OnDragEnter(string text, int x, int y)
        {
            return DropEffect.None;
        }

        public virtual DropEffect OnDragOver(int x, int y, DropEffect effect)
        {
            return DropEffect.None;
        }

        public virtual DropEffect OnDrop(string text, int x, int y, DropEffect effect)
        {
            return DropEffect.None;
        }

        public enum OnErrorEnum : int
        {
            DEFAULT_HANDLER = 0,
            DO_NOTHING = 1,
            CLOSE_APP = 2
        }

        public virtual OnErrorEnum OnError()
        {
            return OnErrorEnum.DEFAULT_HANDLER;
        }

        enum UNDO_REDO_type
        {
            PLG_UNDO = 0,
            PLG_REDO = 1,
            PLG_REMOVE = 2
        };

        public virtual void OnUndoRedoAction(uint action_id, int undo_redo_reset) { }
    }

    #endregion

    /// <summary>
    /// this exception is thrown after on_call_limit event
    /// </summary>
    public class DemoModeCallLimitException : Exception { }


    /// <summary>
    /// Internal implementation of required dll exports for plugin dll file, passes execution to plugin class
    /// documentatin - look at the comments in "ifc_plugin_api.h"
    /// </summary>
    #region DLL exports

    public class PluginDll
    {
        private static readonly Lazy<Plugin> plugin_;
        private static byte[] key;

        static PluginDll()
        {
            string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string pluginName = Assembly.GetExecutingAssembly().GetName().Name;

            string assemblyDllFolder = Path.Combine(
                             Path.Combine(
                            (Directory.GetParent(
                             Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName),
                             "plugins_x64"), pluginName + "_data");

            AddPluginDependencyDirectory(assemblyDllFolder);
            //find first plugin inherited type
            plugin_ = new Lazy<Plugin>(() =>
            {
                var pluginType = typeof(Plugin).Assembly.GetTypes().FirstOrDefault(t => t != typeof(Plugin) && typeof(Plugin).IsAssignableFrom(t));
                return (Plugin)Activator.CreateInstance(pluginType);
            }, true);
        }

        public static void AddPluginDependencyDirectory(string directory)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));
            var assemblyCache = new Dictionary<string, Assembly>(StringComparer.InvariantCultureIgnoreCase);
            AppDomain.CurrentDomain.AssemblyResolve += (o, e) =>
            {
                var assemblyName = new AssemblyName(e.Name);
                var assemblyFile = Path.Combine(directory, assemblyName.Name + ".dll");
                Assembly result;
                if (assemblyCache.TryGetValue(assemblyFile, out result))
                    return result;
                if (File.Exists(assemblyFile))
                {
                    return assemblyCache[assemblyFile] = Assembly.Load(File.ReadAllBytes(assemblyFile));
                }
                else
                    return null;
            };
        }


        protected static Plugin CurrentPlugin { get { return plugin_.Value; } }

        [DllExport("get_api_verion")]
        private static void get_api_verion(ref ApiVersion viewer, ref ApiVersion plugin)
        {
            CurrentPlugin.GetApiVersion(viewer, ref plugin);
        }

        [DllExport("get_plugin_info")]
        private static void get_plugin_info(ref PluginInfo info)
        {
            CurrentPlugin.GetPluginInfo(ref info);
        }

        [DllExport("get_plugin_key")]
        private static IntPtr get_plugin_key()
        {
            key = CurrentPlugin.GetPluginKey();
            if (key != null && key.Length == 292)
                return Marshal.UnsafeAddrOfPinnedArrayElement(key, 0);
            return IntPtr.Zero;
        }

        [DllExport("on_plugin_load")]
        private static void on_plugin_load(PLUGIN_ID plugin_id, bool registered, IntPtr viewer_hwnd)
        {
            key = null;
            CurrentPlugin.OnLoad(plugin_id, registered, viewer_hwnd);
        }

        [DllExport("on_call_limit")]
        private static void on_call_limit()
        {
            CurrentPlugin.OnCallLimit();
            throw new DemoModeCallLimitException();
        }

        [DllExport("on_plugin_unload")]
        private static void on_plugin_unload()
        {
            CurrentPlugin.OnUnload();
        }

        [DllExport("on_gui_colors_change")]
        private static void on_gui_colors_change()
        {
            CurrentPlugin.OnGuiColorsChange();
        }


        [DllExport("on_drag_enter")]
        private static int on_drag_enter([MarshalAs(UnmanagedType.LPStr)] string text, int x, int y)
        {
            return (int)CurrentPlugin.OnDragEnter(text, x, y);
        }


        [DllExport("on_drag_over")]
        private static int on_drag_over(int x, int y, int effect)
        {
            return (int)CurrentPlugin.OnDragOver(x, y, (DropEffect)effect);
        }


        [DllExport("on_drop")]
        private static int on_drop([MarshalAs(UnmanagedType.LPStr)] string text, int x, int y, int effect)
        {
            return (int)CurrentPlugin.OnDrop(text, x, y, (DropEffect)effect);
        }

        [DllExport("on_error")]
        private static int on_error()
        {
            return (int)CurrentPlugin.OnError();
        }

        [DllExport("on_undo_redo_action")]
        private static void on_undo_redo_action(uint action_id, int undo_redo_reset)
        {
            CurrentPlugin.OnUndoRedoAction(action_id, undo_redo_reset);
        }
    }

    #endregion

    /// <summary>
    /// C# wraper for C api defined "ifc_plugin_api.h" 
    /// documentatin - look at the comments in "ifc_plugin_api.h"
    /// </summary>
    #region Low level API


    #region Api Structures

    public struct OBJECT_ID : IEquatable<OBJECT_ID>
    {
        public ulong id;

        public override bool Equals(object obj)
        {
            return obj is OBJECT_ID iD &&
                   id == iD.id;
        }

        public bool Equals(OBJECT_ID other)
        {
            return id == other.id;
        }

        public override int GetHashCode()
        {
            return 1877310944 + id.GetHashCode();
        }

        public static bool operator ==(OBJECT_ID lhs, OBJECT_ID rhs)
        {
            return lhs.id == rhs.id;
        }

        public static bool operator !=(OBJECT_ID lhs, OBJECT_ID rhs)
        {
            return lhs.id != rhs.id;
        }
    }

    public struct SOLID_ID : IEquatable<SOLID_ID>
    {
        public Int64 id;
        public override bool Equals(object obj)
        {
            return obj is SOLID_ID iD &&
            id == iD.id;
        }
        public bool Equals(SOLID_ID other)
        {
            return id == other.id;
        }
        public override int GetHashCode()
        {
            return 1877310944 + id.GetHashCode();
        }

        public static bool operator ==(SOLID_ID lhs, SOLID_ID rhs)
        {
            return lhs.id == rhs.id;
        }
        public static bool operator !=(SOLID_ID lhs, SOLID_ID rhs)
        {
            return lhs.id != rhs.id;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ColorRGB
    {
        public byte r, g, b;
        private byte not_used_padding;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Color
    {
        public byte r, g, b, a;
    }

    public enum VisibleType : int
    {
        vis_invisible,
        vis_visible,
        vis_transparent
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CameraPos
    {
        public double d0, d1, d2, d3, d4, d5, d6, d7, d8, d9; // internal_data
        public char type;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ObjectState
    {
        public VisibleType visible_type;
        [MarshalAs(UnmanagedType.I1)]
        public bool active;
        [MarshalAs(UnmanagedType.I1)]
        public bool selected;
    };

    public enum ObjectType : int
    {
        ot_no_such_id,
        ot_project,
        ot_site,
        ot_building,
        ot_storey,
        ot_element,
    }

    public enum ElementType : int
    {
        et_others,
        et_opening,
        et_beam,
        et_building_element_part,
        et_building_element_proxy,
        et_central_heating,
        et_roof,
        et_door,
        et_elektricity,
        et_gas,
        et_footing,
        et_furniture,
        et_space,
        et_window,
        et_subsoil,
        et_slab,
        et_wall,
        et_curtain_vall,
        et_stair,
        et_column,
        et_air_fitting,
        et_plumbing_drainage,
        et_reinforctment,
        et_pile,
        et_plate,
        et_compound,
        et_components
    }

    public enum GuiTheme : int
    {
        gt_modern_white,
        gt_classic_blue,
        gt_graphite_gray,
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct ObjectInfo_
    {
        public ObjectType object_type;
        public ElementType element_type;
        public IntPtr name;
        public IntPtr description;
        public IntPtr ifc_entity_name;
        public uint ifc_entity_number;
        public OBJECT_ID parent;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ObjectInfo2_
    {
        public OBJECT_ID project;
        public OBJECT_ID building;
        public OBJECT_ID storey;
        public IntPtr tag;
        public IntPtr user_data;
        public IntPtr global_id;
    }



    public struct LAYER_REF
    {
        public Int64 id;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Layer_
    {
        public LAYER_REF layer_ref;
        public IntPtr name;
        public IntPtr description;
    }

    public struct GROUP_REF
    {
        public Int64 id;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Group_
    {
        public GROUP_REF group_ref;
        public IntPtr name;
        public IntPtr description;
        public IntPtr object_type;
        public IntPtr global_id;
    }
    public struct ZONE_REF
    {
        public Int64 id;
    }

    public struct SYSTEM_REF
    {
        public Int64 id;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Zone_
    {
        public ZONE_REF zone_ref;
        public IntPtr name;
        public IntPtr description;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Zone2_
    {
        public ZONE_REF zone_ref;
        public IntPtr name;
        public IntPtr description;
        public IntPtr object_type;
        public IntPtr global_id;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct System_
    {
        public SYSTEM_REF system_ref;
        public IntPtr name;
        public IntPtr description;
        public IntPtr object_type;
        public IntPtr global_id;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PropertySet_
    {
        public int nr;
        public IntPtr name;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct Material_
    {
        public MATERIAL_REF material_ref;
        public IntPtr name;
        public IntPtr description;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct Material2_
    {
        public MATERIAL_REF material_ref;
        public IntPtr name;
        public IntPtr description;
        public IntPtr ifc_entity_name;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct Material3_
    {
        public MATERIAL_REF material_ref;
        public IntPtr name;
        public IntPtr description;
        public IntPtr ifc_entity_name;
        public double layer_thickness;
        public bool is_ventilated;

    };

    [StructLayout(LayoutKind.Sequential)]
    public struct ObjectTypeStyle_
    {
        public OBJECT_TYPE_STYLE_REF typestyle_ref;
        public IntPtr name;
        public IntPtr description;
        public IntPtr ifc_entity_name;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct ObjectTypeStyle2_
    {
        public OBJECT_TYPE_STYLE_REF typestyle_ref;
        public IntPtr name;
        public IntPtr description;
        public IntPtr ifc_entity_name;
        public IntPtr global_id;
    };


    public enum ValueType : int
    {
        vt_string,
        vt_double,
        vt_int,
        vt_bool,
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct Property_
    {
        public int set_nr;
        public int nr;
        public IntPtr name;
        public ValueType value_type;
        public Value_ value;
    };

    [StructLayout(LayoutKind.Explicit)]
    public struct Value_
    {
        [FieldOffset(0)]
        public IntPtr value_str;

        [FieldOffset(0)]
        public double value_num;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Property2_
    {
        public int set_nr;
        public int nr;
        public IntPtr name;
        public ValueType value_type;
        public Value_ value;
        public IntPtr unit;
        public IntPtr reserved;
    };


    public struct REF
    {
        public IntPtr id;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct PropertySetData_
    {
        public int type;
        public REF property_ref;
        public REF set_ref;
        public IntPtr name;
        public IntPtr description;
        public int value_type;
        public IntPtr value_str;
        public double value_num;
        public IntPtr unit;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PropertySetData2_
    {
        public int type;
        public REF property_ref;
        public REF set_ref;
        public IntPtr name;
        public IntPtr description;
        public int value_type;
        public IntPtr value_str;
        public double value_num;
        public IntPtr unit;
        // from API v5.46
        public int level;           // indentation level
        public IntPtr guid;            // only for property sets
        public IntPtr ifc_entity_name;
        public IntPtr reserved;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct PropertyGet
    {
        public OBJECT_ID id;
        public uint set_nr;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PropertyFilter
    {
        public OBJECT_ID id;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string set_name;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string property_name;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Classification_
    {
        public REF cref;
        public IntPtr source;
        public IntPtr edition;
        public IntPtr edition_date;
        public IntPtr name;
        public IntPtr description;
        public IntPtr location;
        public IntPtr reserved;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct ClassificationReference_
    {
        public REF cref;
        public REF cbase;
        public IntPtr location;
        public IntPtr identification;
        public IntPtr name;
        public IntPtr description;
        public IntPtr sort;
    };

    public enum MeasureType : int
    {
        mt_volume,
        mt_area,
        mt_edge,
        mt_vertex,
        mt_centroid,
        mt_surface_distance,
        mt_weight,
        mt_counting,
        mt_coordinates,
        mt_area_curved,
        mt_area_same_normal,
        mt_area_total,
        mt_largest_edge,
        mt_offset,
        mt_point_plane,
        mt_point_edge,
        mt_polygon_area,
        mt_angle_plane,
        mt_angle_edge,
        mt_angle_vertex,
        mt_diameter

    };

    [StructLayout(LayoutKind.Sequential)]
    public struct Measure
    {
        public MeasureType measure_type;
        public double value;
        public double projection_1;
        public double projection_2;
        public double projection_3;
        public double weight_destiny;
        public double weight_units;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct MeasureV1
    {
        public Measure measure;
        public int unit_conv_type;
    };
    [StructLayout(LayoutKind.Sequential)]
    public struct RelativePos
    {
        public OBJECT_ID element_id;
        public double x, y, z;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        public RelativePos pos;
        public Color color;
        public uint size;
        public int draw_style;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct Line
    {
        public RelativePos start, end;
        public Color color;
        public uint width;
        public int style;
        public int draw_style;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AbsoluteLine
    {
        public Vector3d start, end;
        public Color color;
        public uint width;
        public int style;
        public int draw_style;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Image
    {
        public byte[] buffer;
        public uint buffer_size;
        public RelativePos location;        // location of the center of the image relative to product bounding box min. coords
        public Vector3d normal;
        public Vector3d up;
        public double height;               // for scaling - the height of image in the model in [m]
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct AbsoluteImage
    {
        public byte[] buffer;
        public uint buffer_size;
        public Vector3d location;        // location of the center of the image
        public Vector3d normal;
        public Vector3d up;
        public double height;               // for scaling - the height of image in the model in [m]
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct ImageFile
    {
        public string path;
        public RelativePos location;        // location of the center of the image relative to product bounding box min. coords
        public Vector3d normal;
        public Vector3d up;
        public double height;               // for scaling - the height of image in the model in [m]
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct AbsoluteImageFile
    {
        public string path;
        public Vector3d location;        // location of the center of the image
        public Vector3d normal;
        public Vector3d up;
        public double height;               // for scaling - the height of image in the model in [m]
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct Sphere
    {
        public RelativePos pos;
        public Color color;
        public float radius;
        public int segments;
        public int draw_style;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Icon
    {
        public RelativePos pos;
        public int image_nr;
        public uint size;
        public int draw_style;
    };

    public enum LabelStyle : int
    {
        box,
        ballon
    }

    public enum LabelSide : int
    {
        bottom,
        top
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Label
    {
        public ColorRGB bkg_color;
        public RelativePos pos;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string caption;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string txt;
        public LabelStyle style;
        public LabelSide side;
    }

    public enum GuiColorId : int
    {
        // window
        cl_window_light,
        cl_window_medium,       // default tool window background
        cl_window_dark,
        cl_window_super_dark,

        // panels etc
        cl_border_light,
        cl_border_dark,

        // grid
        cl_grid_bkg,
        cl_grid_lines,
        cl_grid_fixed_border_light,
        cl_grid_fixed_border_dark,
        cl_grid_header_text,

        // gradients
        cl_grid_fixed_top,
        cl_grid_fixed_bottom,

        cl_grid_focused_top,
        cl_grid_focused_middle,
        cl_grid_focused_bottom,

        cl_grid_selected_top,
        cl_grid_selected_middle,
        cl_grid_selected_bottom,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    public enum SelectType
    {
        unselect_with_openings,
        select_with_openings,
        select_without_openings
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public double x, y, z;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Face
    {
        public Vertex v1, v2, v3;
        public Vertex normal;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Edge
    {
        public Vertex v1, v2;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3d
    {
        public double x, y, z;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct DirectionCamera
    {
        public Vector3d eye, direction, up;
        public double scale;
        public int fov;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct CuttingPlane
    {
        public Vector3d normal, point;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct Bounds
    {
        public double x_min, y_min, z_min;
        public double x_max, y_max, z_max;
    };

    public struct MATERIAL_REF
    {
        public Int64 id;
    }



    [StructLayout(LayoutKind.Sequential)]
    public struct Material
    {
        public MATERIAL_REF material_ref;
        public string name;
        public string description;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct Material2
    {
        public MATERIAL_REF material_ref;
        public string name;
        public string description;
        public string ifc_entity_name;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct Material3
    {
        public MATERIAL_REF material_ref;
        public string name;
        public string description;
        public string ifc_entity_name;
        public double layer_thickness;
        public bool is_ventilated;

    };

    public struct OBJECT_TYPE_STYLE_REF
    {
        public Int64 id;
    }

    public class ObjectTypeStyle
    {
        public OBJECT_TYPE_STYLE_REF typestyle_ref;
        public string name;
        public string description;
        public string ifc_entity_name;
    };

    public class ObjectTypeStyle2
    {
        public OBJECT_TYPE_STYLE_REF typestyle_ref;
        public string name;
        public string description;
        public string ifc_entity_name;
        public string global_id;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct Pyramid
    {
        public RelativePos apex;               // position
        public float pan_angle;  //yaw   // in degree
        public float tilt_angle; //pitch // in degree
        public float roll_angle;
        public float height;             // in meters

        public float horizontal_angle; // in degree
        public float vertical_angle;       // in degree

        public int style;              // 0
        public int draw_style;         // 0
        public Color face_color;
        public Color line_color;
    };

    public enum PluginStatusEnum : int
    {
        ps_non_exist,   // there no such plugin installed
        ps_not_loaded,  // plugin is installed but disabled
        ps_demo_mode,   // plugin is loaded and in demo mode
        ps_full_version,// plugin is loaded nad in full mode
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct PluginStatus
    {
        public PluginStatusEnum status; // PluginStatusEnum
        public int v_major, v_minor;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct PluginStatus2
    {
        public int status; // PluginStatusEnum
        public int v_major, v_minor, v_release, v_build;
        public bool v_pre_release; // Indicates whether the plugin version is a pre-release version
    };

    [StructLayout(LayoutKind.Explicit)]
    public struct Msg
    {
        [FieldOffset(0)]
        public int type;

        [FieldOffset(0)]
        public int result;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct MsgParam
    {
        [FieldOffset(0)]
        public OBJECT_ID object_id;

        [FieldOffset(0)]
        public IntPtr str;  //TODO: convert from and to string

        [FieldOffset(0)]
        public uint count;

        [FieldOffset(0)]
        public IntPtr objects;  //TODO: convert from and to array of OBJECT_ID

        [FieldOffset(0)]
        public IntPtr pointer;

        [FieldOffset(0)]
        public uint pointerSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PluginMessage
    {
        public Msg msg;
        public MsgParam param1, param2, param3;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MeasureDetail
    {
        public double value;
        public OBJECT_ID object_id;
        public OBJECT_ID second_object_id;
    }

    public struct Plane
    {
        public Vector3d normal;
        public double d;
    }

    public struct Segment
    {
        public Vector3d first, second;
    }


    public struct ConstructMeasureState_
    {
        public MeasureType type;

        public OBJECT_ID id;
        public Plane plane;
        public Segment segment;
    }


    public struct PropertyData
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string name;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string description;

        public ValueType value_type;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string value_str;
        public double value_num;

        [MarshalAs(UnmanagedType.LPStr)]
        public string unit;


    };



    public struct PropertyDef
    {
        public OBJECT_ID object_id;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string set_name;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string name;
    }

    public enum ScreenshotStyle : int
    {
        style0 = 0,
        style1 = 1,
        style2 = 2,
    }

    [Flags]
    public enum ScreenshotFlags : int
    {
        SSF_WHITE_BACKGROUND = 0x4,
        SSF_WITHOUT_PLUGINS = 0x8,
        SSF_DISABLE_ANTIALIASING = 0x10,
        SSF_BOLD_LINES = 0x20,
        SSF_DISABLE_LIGHTING = 0x40,
    }

    public enum CrossSetionColor
    {
        csc_object = 0,
        csc_original = 1,
        csc_own = 2,
    }

    public enum SaveType
    {
        st_bvf = 0,
        st_cache = 1,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ProjectOffset
    {
        public OBJECT_ID id;
        Vector3d offset;
    }

    public struct MeasuredArea
    {
        public double max0_area;      // out
        public double max1_area;      // out
        public double min1_area;      // out
        public double min0_area;      // out
        public double total_area;     // out
    };

    // additional options associated with assigned color
    public enum ColorOption : int
    {
        co_none = 0,         // no additional options
        co_ignore_alpha = 1, // alpha channel associated with assigned color is ignored when rendering object
    };

    // options defining what part of the object is retrieved color's source
    public enum ColorSource : int
    {
        cs_face = 1, // color is retrieved from object's faces
        cs_edge = 2, // color is retrieved from object's edges
    };

    // options defining what kind of color should be retrieved
    public enum RetrieveColorOption : int
    {
        rco_none = 0,     // no color
        rco_current = 1,  // current color as seen on the screen, affected by viewer transparency level but without selection or measurements effects, can be retrieved at any time
        rco_default = 2,  // default color as defined internally in the viewer for particular type of object, if exists can be retrieved even if not seen on the screen as a current color, not affected by viewer color options
        rco_ifc = 4,      // color read from the ifc file as object's own color or based on object's ifc dependencies, if exists can be retrieved even if not seen on the screen as a current color, might be affected by viewer color options
        rco_assigned = 8, // external color assigned to the object, e.g. by plugin, can be retrieved only if previously assigned, not affected by viewer color options
    };

    enum DeactivateColorOption : int
    {
        dco_none = 0,          // no additional actions
        dco_with_children = 1, // affect also all target object's children
    };

    #endregion

    /// <summary>
    /// documentatin - look at the comments in "ifc_plugin_api.h"
    /// </summary>
    public class Api
    {
        public static fun_get_selected_id get_selected_id;
        public static fun_get_all_objects_id get_all_objects_id;

        public static fun_set_color set_color;
        public static fun_set_color_object set_color_object;
        public static fun_set_default_color set_default_color;
        public static fun_use_selection_color use_selection_color;

        public static fun_treat_openings_as_normal_objects treat_openings_as_normal_objects;
        public static fun_set_drag_mode set_drag_mode;

        public static fun_set_visible set_visible;
        public static fun_set_visible_object set_visible_object;
        public static fun_set_visible_many set_visible_many;
        public static fun_set_visible_many_objects set_visible_many_objects;

        public static fun_set_object_color set_object_color;
        public static fun_set_object_color_rgb set_object_color_rgb;
        public static fun_set_object_default_color set_object_default_color;
        public static fun_get_object_color get_object_color;
        public static fun_get_object_deafult_color get_object_deafult_color;

        public static fun_retrieve_color retrieve_color;
        public static fun_assign_color assign_color;
        public static fun_deactivate_color deactivate_color;

        public static fun_get_transparency get_transparency;
        public static fun_add_color_to_transparency_list add_color_to_transparency_list;
        public static fun_set_object_edge_color set_object_edge_color;
        public static fun_set_object_edge_default_color set_object_edge_default_color;

        public static fun_set_object_visible set_object_visible;
        public static fun_set_object_active set_object_active;

        public static fun_select select;
        public static fun_unselect_openings unselect_openings;
        public static fun_select_many select_many;
        public static fun_apply_select_rules apply_select_rules;
        public static fun_close_application close_application;

        public static fun_zoom_to zoom_to;
        public static fun_zoom_to_in_view zoom_to_in_view;
        public static fun_zoom_to_box zoom_to_box;
        public static fun_get_camera_pos get_camera_pos;
        public static fun_set_camera_pos set_camera_pos;
        public static fun_invalidate invalidate;
        public static fun_get_draw_time get_draw_time;

        public static fun_get_all_objects get_all_objects;
        public static fun_get_selected get_selected;
        public static fun_get_visible get_visible;
        public static fun_get_object_state get_object_state;

        public static fun_is_visible is_visible;
        public static fun_is_transparent is_transparent;
        public static fun_is_selected is_selected;
        public static fun_is_active is_active;

        public static fun_get_childs get_childs;
        public static fun_get_object_info get_object_info;

        public static fun_set_user_data set_user_data;
        public static fun_get_user_data get_user_data;
        public static fun_get_project get_project;
        public static fun_get_building get_building;
        public static fun_get_storey get_storey;
        public static fun_get_parent get_parent;
        public static fun_get_object_info2 get_object_info2;

        public static fun_get_layers get_layers;
        public static fun_get_layer_objects get_layer_objects;
        public static fun_get_zones get_zones;
        public static fun_get_zones2 get_zones2;
        public static fun_get_zone_objects get_zone_objects;

        public static fun_get_systems get_systems;
        public static fun_get_system_objects get_system_objects;
        public static fun_delete_object_property_or_set delete_object_property_or_set;
        public static fun_add_or_change_object_property add_or_change_object_property;

        public static fun_get_groups get_groups;
        public static fun_get_group_objects get_group_objects;

        public static fun_get_property_sets get_property_sets;
        public static fun_get_properties get_properties;
        public static fun_filter_properties filter_properties;
        public static fun_select_property select_property;

        public static fun_get_object_properties get_object_properties;
        public static fun_get_object_properties2 get_object_properties2;

        public static fun_get_material_properties get_material_properties;
        public static fun_get_type_style_properties get_type_style_properties;

        public static fun_get_object_classification_references get_object_classification_references;
        public static fun_get_classification_data get_classification_data;
        public static fun_get_classification_reference_data get_classification_reference_data;
        public static fun_get_classifications get_classifications;
        public static fun_get_classification_references get_classification_references;

        public static fun_get_unit_factor get_unit_factor;
        public static fun_first_geometry first_geometry;
        public static fun_get_total_geometry_bounds get_total_geometry_bounds;
        public static fun_get_geometry get_geometry;
        public static fun_get_geometry_color get_geometry_color;
        public static fun_get_geometry_edges get_geometry_edges;
        public static fun_create_user_object create_user_object;
        public static fun_delete_user_object delete_user_object;
        public static fun_check_triangle check_triangle;
        public static fun_add_geometry_user_object add_geometry_user_object;
        public static fun_delete_geometry_user_object delete_geometry_user_object;
        public static fun_set_geometry_edges_user_object set_geometry_edges_user_object;
        public static fun_next_geometry next_geometry;
        public static fun_is_online_licence is_online_licence;

        //public static 
        public static fun_on_property_change on_property_change;

        public static fun_get_object_corners get_object_corners;
        public static fun_get_object_edges get_object_edges;
        public static fun_get_object_area get_object_area;

        public static fun_on_measure_change on_measure_change;
        public static fun_get_measure get_measure;
        public static fun_get_measure_v1 get_measure_v1;
        public static fun_get_measure_objects get_measure_objects;
        public static fun_get_measure_elements get_measure_elements;
        public static fun_clear_measure clear_measure;

        public static fun_get_loaded_ifc_path get_loaded_ifc_path;
        public static fun_get_project_path get_project_path;
        public static fun_extract_file_from_bvf extract_file_from_bvf;
        public static fun_load_ifc load_ifc;
        public static fun_load_ifc_from_buffer load_ifc_from_buffer;
        public static fun_calculate_object_id calculate_object_id;

        public static fun_on_model_load on_model_load;
        public static fun_on_model_load_v2 on_model_load_v2;
        public static fun_on_model_save_v2 on_model_save_v2;
        public static fun_on_model_clear on_model_clear;
        public static fun_on_main_form_close on_main_form_close;
        public static fun_on_selection_change on_selection_change;
        public static fun_get_clicked_pos get_clicked_pos;
        public static fun_get_clicked_normal get_clicked_normal;
        public static fun_on_draw on_draw;
        public static fun_set_draw_object_id set_draw_object_id;
        public static fun_on_draw_object_click on_draw_object_click;

        public static fun_on_undo_redo_action on_undo_redo_action;

        public static fun_get_draw_object_id get_draw_object_id;

        public static fun_draw_point draw_point;
        public static fun_draw_line draw_line;
        public static fun_draw_absolute_line draw_absolute_line;
        public static fun_draw_image draw_image;
        public static fun_draw_absolute_image draw_absolute_image;
        public static fun_draw_image_file draw_image_file;
        public static fun_draw_absolute_image_file draw_absolute_image_file;
        public static fun_draw_sphere draw_sphere;

        public static fun_load_icon_image load_icon_image;
        public static fun_draw_icon draw_icon;

        public static fun_draw_label draw_label;

        public static fun_get_element_pos get_element_pos;

        public static fun_save_file_as save_file_as;

        public static fun_get_language get_language;
        public static fun_load_texts load_texts;
        public static fun_get_text get_text;
        public static fun_get_text_global get_text_global;

        public static fun_get_entity_type_name get_entity_type_name;

        public static fun_get_object_below_mouse get_object_below_mouse;

        public static fun_register_undo_action register_undo_action;

        public static fun_create_tab create_tab;
        public static fun_create_group create_group;
        public static fun_show_group show_group;

        public static fun_create_button create_button;
        public static fun_create_small_button create_small_button;
        public static fun_create_checkbox create_checkbox;
        public static fun_create_radio_button create_radio_button;
        public static fun_create_dropdown_button create_dropdown_button;
        public static fun_create_sub_button create_sub_button;
        public static fun_create_sub_button2 create_sub_button2;
        public static fun_create_separator create_separator;
        public static fun_add_button add_button;
        public static fun_add_separator add_separator;

        public static fun_set_button_text set_button_text;
        public static fun_set_button_image set_button_image;
        public static fun_set_button_small_image set_button_small_image;
        public static fun_set_button_shortcut set_button_shortcut;

        public static fun_set_button_state set_button_state;
        public static fun_set_button_down set_button_down;
        public static fun_set_button_guid set_button_guid;
        public static fun_enable_button enable_button;
        public static fun_show_button show_button;

        public static fun_clicked_button clicked_button;
        public static fun_begin_control_group begin_control_group;
        public static fun_on_context_menu on_context_menu;
        public static fun_add_context_button add_context_button;
        public static fun_add_context_button_with_separator add_context_button_with_separator;
        public static fun_clear_context_menu clear_context_menu;

        public static fun_create_gallery_button create_gallery_button;
        public static fun_set_gallery_style set_gallery_style;
        public static fun_create_gallery_category create_gallery_category;
        public static fun_set_gallery_category_text set_gallery_category_text;
        public static fun_set_gallery_category_style set_gallery_category_style;
        public static fun_delete_gallery_category delete_gallery_category;
        public static fun_create_gallery_item create_gallery_item;
        public static fun_set_gallery_item_text set_gallery_item_text;
        public static fun_set_gallery_item_image set_gallery_item_image;
        public static fun_delete_gallery_item delete_gallery_item;
        public static fun_clear_gallery clear_gallery;
        public static fun_on_gallery_item_context_menu on_gallery_item_context_menu;

        public static fun_is_center_view is_center_view;

        public static fun_get_active_tab get_active_tab;
        public static fun_on_tab_change on_tab_change;

        public static fun_get_gui_color get_gui_color;
        public static fun_get_view_rect get_view_rect;
        public static fun_show_ifc_structure_grid show_ifc_structure_grid;
        public static fun_set_ribbon_state set_ribbon_state;
        public static fun_set_grids_state set_grids_state;
        public static fun_get_grids_state get_grids_state;
        public static fun_is_touch_mode is_touch_mode;

        public static fun_get_view_state get_view_state;
        public static fun_set_view_state set_view_state;

        public static fun_render_screenshot render_screenshot;
        public static fun_set_screenshot_style set_screenshot_style;

        public static fun_get_2d_bounds get_2d_bounds;
        public static fun_project_to_2d project_to_2d;
        public static fun_recalc_cross_sections recalc_cross_sections;
        public static fun_set_cross_sections_style set_cross_sections_style;

        public static fun_get_volume get_volume;
        public static fun_get_centroid get_centroid;

        public static fun_get_direction_camera get_direction_camera;
        public static fun_set_direction_camera set_direction_camera;

        public static fun_set_measure_type set_measure_type;
        public static fun_get_total_area get_total_area;

        public static fun_begin_read_bvf begin_read_bvf;
        public static fun_begin_read_bvf_v2 begin_read_bvf_v2;

        public static fun_read_bvf read_bvf;
        public static fun_read_bvf_v2 read_bvf_v2;

        public static fun_save_bvf_sub_file_v2 save_bvf_sub_file_v2;
        public static fun_get_bvf_dir_file_list_v2 get_bvf_dir_file_list_v2;

        public static fun_get_bvf_size get_bvf_size;
        public static fun_get_bvf_size_v2 get_bvf_size_v2;

        public static fun_on_save_bvf on_save_bvf;

        public static fun_begin_write_bvf begin_write_bvf;
        public static fun_begin_write_bvf_v2 begin_write_bvf_v2;

        public static fun_write_bvf write_bvf;
        public static fun_write_bvf_v2 write_bvf_v2;
        public static fun_write_file_to_bvf_v2 write_file_to_bvf_v2;

        public static fun_can_clear_model can_clear_model;

        public static fun_file_was_added file_was_added;
        public static fun_need_save_bvf need_save_bvf;
        public static fun_save_file save_file;
        public static fun_need_save_changes need_save_changes;

        public static fun_on_hover on_hover;
        public static fun_set_hover_objects set_hover_objects;
        public static fun_get_hovered_pos get_hovered_pos;
        public static fun_get_dropped_pos get_dropped_pos;

        public static fun_get_cutting_planes get_cutting_planes;
        public static fun_set_cutting_planes set_cutting_planes;
        public static fun_get_active get_active;
        public static fun_get_current_product get_current_product;

        public static fun_has_representation has_representation;
        public static fun_get_bounds get_bounds;
        public static fun_get_offset get_offset;
        public static fun_set_offset set_offset;
        public static fun_get_object_materials get_object_materials;
        public static fun_get_object_materials2 get_object_materials2;
        public static fun_get_object_materials3 get_object_materials3;
        public static fun_get_object_type_style get_object_type_style;
        public static fun_get_object_type_style2 get_object_type_style2;
        public static fun_draw_pyramid draw_pyramid;

        public static fun_create_intersection_solid create_intersection_solid;
        public static fun_delete_solid delete_solid;
        public static fun_draw_solid draw_solid;
        public static fun_get_solid_volume get_solid_volume;
        public static fun_get_solid_area get_solid_area;
        public static fun_get_solid_center_point get_solid_center_point;
        public static fun_zoom_to_solids zoom_to_solids;

        public static fun_zoom_to_objects zoom_to_objects;

        public static fun_get_gui_theme get_gui_theme;

        public static fun_message_box message_box;
        public static fun_set_open_dialog_params set_open_dialog_params;
        public static fun_open_file_dialog open_file_dialog;
        public static fun_get_open_dialog_file_count get_open_dialog_file_count;
        public static fun_get_open_dialog_file_nr get_open_dialog_file_nr;
        public static fun_set_save_dialog_params set_save_dialog_params;
        public static fun_set_save_dialog_default_extension set_save_dialog_default_extension;
        public static fun_save_dialog save_dialog;
        public static fun_get_save_dialog_name get_save_dialog_name;
        public static fun_show_progress_bar show_progress_bar;
        public static fun_update_progress_bar update_progress_bar;

        public static fun_get_properties2 get_properties2;
        public static fun_on_tab_sheet_placement on_tab_sheet_placement;
        public static fun_on_tab_sheet_change on_tab_sheet_change;

        public static fun_get_measure_state get_measure_state;
        public static fun_set_measure_state set_measure_state;

        public static fun_get_measure_count get_measure_count;
        public static fun_get_detailed_measure_state get_detailed_measure_state;
        public static fun_set_detailed_measure_state set_detailed_measure_state;
        public static fun_change_measure_state_density change_measure_state_density;
        public static fun_construct_measure_state construct_measure_state;

        public static fun_on_object_list_changed on_object_list_changed;


        public static fun_get_loaded_ifc_files_count get_loaded_ifc_files_count;
        public static fun_get_loaded_ifc_filename get_loaded_ifc_filename;

        public static fun_get_plugin_status get_plugin_status;
        public static fun_get_plugin_status2 get_plugin_status2;
        public static fun_send_plugin_massage send_plugin_massage;
        public static fun_on_plugin_message on_plugin_message;

        public static fun_create_tab_sheet create_tab_sheet;
        public static fun_activate_tab_sheet activate_tab_sheet;
        public static fun_close_tab_sheet close_tab_sheet;
        public static fun_show_tab_sheet show_tab_sheet;
        public static fun_is_active_tab_sheet is_active_tab_sheet;

        public static fun_add_ifc add_ifc;
        public static fun_item_command item_command;
        public static fun_set_item_value set_item_value;
        public static fun_get_item_value get_item_value;

        public static fun_register_on_button_click register_on_button_click;

        public static fun_get_projects_offsets get_projects_offsets;

        //------------------------------------------------------------------------------
        //------------------------------UTILITY-----------------------------------------
        //------------------------------------------------------------------------------
        public static fun_bv_zip_add bv_zip_add;
        public static fun_bv_zip_get bv_zip_get;
        public static fun_bv_zip_get_file_length bv_zip_get_file_length;
        public static fun_bv_zip_read_buffer bv_zip_read_buffer;
        public static fun_bv_zip_write_buffer bv_zip_write_buffer;

        public static void loadPluginFunctions()
        {
            get_selected_id = (fun_get_selected_id)LoadFunction<fun_get_selected_id>("get_selected_id");
            get_all_objects_id = (fun_get_all_objects_id)LoadFunction<fun_get_all_objects_id>("get_all_objects_id");

            set_color = (fun_set_color)LoadFunction<fun_set_color>("set_color");
            set_color_object = (fun_set_color_object)LoadFunction<fun_set_color_object>("set_color_object");
            set_default_color = (fun_set_default_color)LoadFunction<fun_set_default_color>("set_default_color");
            use_selection_color = (fun_use_selection_color)LoadFunction<fun_use_selection_color>("use_selection_color");
            treat_openings_as_normal_objects = (fun_treat_openings_as_normal_objects)LoadFunction<fun_treat_openings_as_normal_objects>("treat_openings_as_normal_objects");

            set_drag_mode = (fun_set_drag_mode)LoadFunction<fun_set_drag_mode>("set_drag_mode");

            set_visible = (fun_set_visible)LoadFunction<fun_set_visible>("set_visible");
            set_visible_object = (fun_set_visible_object)LoadFunction<fun_set_visible_object>("set_visible_object");
            set_visible_many = (fun_set_visible_many)LoadFunction<fun_set_visible_many>("set_visible_many");
            set_visible_many_objects = (fun_set_visible_many_objects)LoadFunction<fun_set_visible_many_objects>("set_visible_many_objects");

            set_object_color = (fun_set_object_color)LoadFunction<fun_set_object_color>("set_object_color");
            set_object_color_rgb = (fun_set_object_color_rgb)LoadFunction<fun_set_object_color_rgb>("set_object_color_rgb");
            set_object_default_color = (fun_set_object_default_color)LoadFunction<fun_set_object_default_color>("set_object_default_color");
            get_object_color = (fun_get_object_color)LoadFunction<fun_get_object_color>("get_object_color");
            get_object_deafult_color = (fun_get_object_deafult_color)LoadFunction<fun_get_object_deafult_color>("get_object_deafult_color");

            retrieve_color = (fun_retrieve_color)LoadFunction<fun_retrieve_color>("retrieve_color");
            assign_color = (fun_assign_color)LoadFunction<fun_assign_color>("assign_color");
            deactivate_color = (fun_deactivate_color)LoadFunction<fun_deactivate_color>("deactivate_color");

            get_transparency = (fun_get_transparency)LoadFunction<fun_get_transparency>("get_transparency");
            add_color_to_transparency_list = (fun_add_color_to_transparency_list)LoadFunction<fun_add_color_to_transparency_list>("add_color_to_transparency_list");
            set_object_edge_color = (fun_set_object_edge_color)LoadFunction<fun_set_object_edge_color>("set_object_edge_color");
            set_object_edge_default_color = (fun_set_object_edge_default_color)LoadFunction<fun_set_object_edge_default_color>("set_object_edge_default_color");

            set_object_visible = (fun_set_object_visible)LoadFunction<fun_set_object_visible>("set_object_visible");
            set_object_active = (fun_set_object_active)LoadFunction<fun_set_object_active>("set_object_active");

            select = (fun_select)LoadFunction<fun_select>("select");
            unselect_openings = (fun_unselect_openings)LoadFunction<fun_unselect_openings>("unselect_openings");
            select_many = (fun_select_many)LoadFunction<fun_select_many>("select_many");
            apply_select_rules = (fun_apply_select_rules)LoadFunction<fun_apply_select_rules>("apply_select_rules");
            close_application = (fun_close_application)LoadFunction<fun_close_application>("close_application");

            zoom_to = (fun_zoom_to)LoadFunction<fun_zoom_to>("zoom_to");
            zoom_to_in_view = (fun_zoom_to_in_view)LoadFunction<fun_zoom_to_in_view>("zoom_to_in_view");
            zoom_to_box = (fun_zoom_to_box)LoadFunction<fun_zoom_to_box>("zoom_to_box");
            get_camera_pos = (fun_get_camera_pos)LoadFunction<fun_get_camera_pos>("get_camera_pos");
            set_camera_pos = (fun_set_camera_pos)LoadFunction<fun_set_camera_pos>("set_camera_pos");
            calculate_object_id = (fun_calculate_object_id)LoadFunction<fun_calculate_object_id>("calculate_object_id");
            invalidate = (fun_invalidate)LoadFunction<fun_invalidate>("invalidate");
            get_draw_time = (fun_get_draw_time)LoadFunction<fun_get_draw_time>("get_draw_time");

            get_all_objects = (fun_get_all_objects)LoadFunction<fun_get_all_objects>("get_all_objects");
            get_selected = (fun_get_selected)LoadFunction<fun_get_selected>("get_selected");
            get_visible = (fun_get_visible)LoadFunction<fun_get_visible>("get_visible");
            get_object_state = (fun_get_object_state)LoadFunction<fun_get_object_state>("get_object_state");

            is_visible = (fun_is_visible)LoadFunction<fun_is_visible>("is_visible");
            is_transparent = (fun_is_transparent)LoadFunction<fun_is_transparent>("is_transparent");
            is_selected = (fun_is_selected)LoadFunction<fun_is_selected>("is_selected");
            is_active = (fun_is_active)LoadFunction<fun_is_active>("is_active");

            get_childs = (fun_get_childs)LoadFunction<fun_get_childs>("get_childs");
            get_object_info = (fun_get_object_info)LoadFunction<fun_get_object_info>("get_object_info");

            set_user_data = (fun_set_user_data)LoadFunction<fun_set_user_data>("set_user_data");
            get_user_data = (fun_get_user_data)LoadFunction<fun_get_user_data>("get_user_data");
            get_project = (fun_get_project)LoadFunction<fun_get_project>("get_project");
            get_building = (fun_get_building)LoadFunction<fun_get_building>("get_building");
            get_storey = (fun_get_storey)LoadFunction<fun_get_storey>("get_storey");
            get_parent = (fun_get_parent)LoadFunction<fun_get_parent>("get_parent");
            get_object_info2 = (fun_get_object_info2)LoadFunction<fun_get_object_info2>("get_object_info2");

            get_layers = (fun_get_layers)LoadFunction<fun_get_layers>("get_layers");
            get_layer_objects = (fun_get_layer_objects)LoadFunction<fun_get_layer_objects>("get_layer_objects");
            get_zones = (fun_get_zones)LoadFunction<fun_get_zones>("get_zones");
            get_zones2 = (fun_get_zones2)LoadFunction<fun_get_zones2>("get_zones2");
            get_zone_objects = (fun_get_zone_objects)LoadFunction<fun_get_zone_objects>("get_zone_objects");

            get_systems = (fun_get_systems)LoadFunction<fun_get_systems>("get_systems");
            get_system_objects = (fun_get_system_objects)LoadFunction<fun_get_system_objects>("get_system_objects");

            delete_object_property_or_set = (fun_delete_object_property_or_set)LoadFunction<fun_delete_object_property_or_set>("delete_object_property_or_set");
            add_or_change_object_property = (fun_add_or_change_object_property)LoadFunction<fun_add_or_change_object_property>("add_or_change_object_property");


            get_groups = (fun_get_groups)LoadFunction<fun_get_groups>("get_groups");
            get_group_objects = (fun_get_group_objects)LoadFunction<fun_get_group_objects>("get_group_objects");

            get_property_sets = (fun_get_property_sets)LoadFunction<fun_get_property_sets>("get_property_sets");
            get_properties = (fun_get_properties)LoadFunction<fun_get_properties>("get_properties");
            filter_properties = (fun_filter_properties)LoadFunction<fun_filter_properties>("filter_properties");
            select_property = (fun_select_property)LoadFunction<fun_select_property>("select_property");

            get_object_properties = (fun_get_object_properties)LoadFunction<fun_get_object_properties>("get_object_properties");
            get_object_properties2 = (fun_get_object_properties2)LoadFunction<fun_get_object_properties2>("get_object_properties2");
            get_material_properties = (fun_get_material_properties)LoadFunction<fun_get_material_properties>("get_material_properties");
            get_type_style_properties = (fun_get_type_style_properties)LoadFunction<fun_get_type_style_properties>("get_type_style_properties");

            get_object_classification_references = (fun_get_object_classification_references)LoadFunction<fun_get_object_classification_references>("get_object_classification_references");
            get_classification_data = (fun_get_classification_data)LoadFunction<fun_get_classification_data>("get_classification_data");
            get_classification_reference_data = (fun_get_classification_reference_data)LoadFunction<fun_get_classification_reference_data>("get_classification_reference_data");
            get_classifications = (fun_get_classifications)LoadFunction<fun_get_classifications>("get_classifications");
            get_classification_references = (fun_get_classification_references)LoadFunction<fun_get_classification_references>("get_classification_references");

            get_unit_factor = (fun_get_unit_factor)LoadFunction<fun_get_unit_factor>("get_unit_factor");
            first_geometry = (fun_first_geometry)LoadFunction<fun_first_geometry>("first_geometry");
            get_total_geometry_bounds = (fun_get_total_geometry_bounds)LoadFunction<fun_get_total_geometry_bounds>("get_total_geometry_bounds");
            get_geometry = (fun_get_geometry)LoadFunction<fun_get_geometry>("get_geometry");
            get_geometry_color = (fun_get_geometry_color)LoadFunction<fun_get_geometry_color>("get_geometry_color");
            get_geometry_edges = (fun_get_geometry_edges)LoadFunction<fun_get_geometry_edges>("get_geometry_edges");
            create_user_object = (fun_create_user_object)LoadFunction<fun_create_user_object>("create_user_object");
            add_geometry_user_object = (fun_add_geometry_user_object)LoadFunction<fun_add_geometry_user_object>("add_geometry_user_object");
            delete_geometry_user_object = (fun_delete_geometry_user_object)LoadFunction<fun_delete_geometry_user_object>("delete_geometry_user_object");
            check_triangle = (fun_check_triangle)LoadFunction<fun_check_triangle>("check_triangle");
            delete_user_object = (fun_delete_user_object)LoadFunction<fun_delete_user_object>("delete_user_object");
            set_geometry_edges_user_object = (fun_set_geometry_edges_user_object)LoadFunction<fun_set_geometry_edges_user_object>("set_geometry_edges_user_object");
            next_geometry = (fun_next_geometry)LoadFunction<fun_next_geometry>("next_geometry");
            is_online_licence = (fun_is_online_licence)LoadFunction<fun_is_online_licence>("is_online_licence");

            on_property_change = (fun_on_property_change)LoadFunction<fun_on_property_change>("on_property_change");

            get_object_corners = (fun_get_object_corners)LoadFunction<fun_get_object_corners>("get_object_corners");
            get_object_edges = (fun_get_object_edges)LoadFunction<fun_get_object_edges>("get_object_edges");
            get_object_area = (fun_get_object_area)LoadFunction<fun_get_object_area>("get_object_area");

            on_measure_change = (fun_on_measure_change)LoadFunction<fun_on_measure_change>("on_measure_change");
            get_measure = (fun_get_measure)LoadFunction<fun_get_measure>("get_measure");
            get_measure_v1 = (fun_get_measure_v1)LoadFunction<fun_get_measure_v1>("get_measure_v1");
            get_measure_objects = (fun_get_measure_objects)LoadFunction<fun_get_measure_objects>("get_measure_objects");
            get_measure_elements = (fun_get_measure_elements)LoadFunction<fun_get_measure_elements>("get_measure_elements");
            clear_measure = (fun_clear_measure)LoadFunction<fun_clear_measure>("clear_measure");

            get_loaded_ifc_path = (fun_get_loaded_ifc_path)LoadFunction<fun_get_loaded_ifc_path>("get_loaded_ifc_path");
            get_project_path = (fun_get_project_path)LoadFunction<fun_get_project_path>("get_project_path");
            extract_file_from_bvf = (fun_extract_file_from_bvf)LoadFunction<fun_extract_file_from_bvf>("extract_file_from_bvf");
            load_ifc = (fun_load_ifc)LoadFunction<fun_load_ifc>("load_ifc");
            load_ifc_from_buffer = (fun_load_ifc_from_buffer)LoadFunction<fun_load_ifc_from_buffer>("load_ifc_from_buffer");

            on_model_load = (fun_on_model_load)LoadFunction<fun_on_model_load>("on_model_load");
            on_model_load_v2 = (fun_on_model_load_v2)LoadFunction<fun_on_model_load_v2>("on_model_load_v2");
            on_model_save_v2 = (fun_on_model_save_v2)LoadFunction<fun_on_model_save_v2>("on_model_save_v2");
            on_model_clear = (fun_on_model_clear)LoadFunction<fun_on_model_clear>("on_model_clear");
            on_main_form_close = (fun_on_main_form_close)LoadFunction<fun_on_main_form_close>("on_main_form_close");
            on_selection_change = (fun_on_selection_change)LoadFunction<fun_on_selection_change>("on_selection_change");
            get_clicked_pos = (fun_get_clicked_pos)LoadFunction<fun_get_clicked_pos>("get_clicked_pos");
            get_clicked_normal = (fun_get_clicked_normal)LoadFunction<fun_get_clicked_normal>("get_clicked_normal");
            on_draw = (fun_on_draw)LoadFunction<fun_on_draw>("on_draw");
            set_draw_object_id = (fun_set_draw_object_id)LoadFunction<fun_set_draw_object_id>("set_draw_object_id");
            on_draw_object_click = (fun_on_draw_object_click)LoadFunction<fun_on_draw_object_click>("on_draw_object_click");

            on_undo_redo_action = (fun_on_undo_redo_action)LoadFunction<fun_on_undo_redo_action>("on_undo_redo_action");

            get_draw_object_id = (fun_get_draw_object_id)LoadFunction<fun_get_draw_object_id>("get_draw_object_id");

            draw_point = (fun_draw_point)LoadFunction<fun_draw_point>("draw_point");
            draw_line = (fun_draw_line)LoadFunction<fun_draw_line>("draw_line");
            draw_absolute_line = (fun_draw_absolute_line)LoadFunction<fun_draw_absolute_line>("draw_absolute_line");
            draw_image = (fun_draw_image)LoadFunction<fun_draw_image>("draw_image");
            draw_absolute_image = (fun_draw_absolute_image)LoadFunction<fun_draw_absolute_image>("draw_absolute_image");
            draw_image_file = (fun_draw_image_file)LoadFunction<fun_draw_image_file>("draw_image_file");
            draw_absolute_image_file = (fun_draw_absolute_image_file)LoadFunction<fun_draw_absolute_image_file>("draw_absolute_image_file");
            draw_sphere = (fun_draw_sphere)LoadFunction<fun_draw_sphere>("draw_sphere");

            load_icon_image = (fun_load_icon_image)LoadFunction<fun_load_icon_image>("load_icon_image");
            draw_icon = (fun_draw_icon)LoadFunction<fun_draw_icon>("draw_icon");

            draw_label = (fun_draw_label)LoadFunction<fun_draw_label>("draw_label");

            get_element_pos = (fun_get_element_pos)LoadFunction<fun_get_element_pos>("get_element_pos");

            save_file_as = (fun_save_file_as)LoadFunction<fun_save_file_as>("save_file_as");

            get_language = (fun_get_language)LoadFunction<fun_get_language>("get_language");
            load_texts = (fun_load_texts)LoadFunction<fun_load_texts>("load_texts");
            get_text = (fun_get_text)LoadFunction<fun_get_text>("get_text");
            get_text_global = (fun_get_text_global)LoadFunction<fun_get_text_global>("get_text_global");

            get_entity_type_name = (fun_get_entity_type_name)LoadFunction<fun_get_entity_type_name>("get_entity_type_name");

            get_object_below_mouse = (fun_get_object_below_mouse)LoadFunction<fun_get_object_below_mouse>("get_object_below_mouse");

            register_undo_action = (fun_register_undo_action)LoadFunction<fun_register_undo_action>("register_undo_action");

            create_tab = (fun_create_tab)LoadFunction<fun_create_tab>("create_tab");
            create_group = (fun_create_group)LoadFunction<fun_create_group>("create_group");
            show_group = (fun_show_group)LoadFunction<fun_show_group>("show_group");

            create_button = (fun_create_button)LoadFunction<fun_create_button>("create_button");
            create_small_button = (fun_create_small_button)LoadFunction<fun_create_small_button>("create_small_button");
            create_checkbox = (fun_create_checkbox)LoadFunction<fun_create_checkbox>("create_checkbox");
            create_radio_button = (fun_create_radio_button)LoadFunction<fun_create_radio_button>("create_radio_button");
            create_dropdown_button = (fun_create_dropdown_button)LoadFunction<fun_create_dropdown_button>("create_dropdown_button");
            create_sub_button = (fun_create_sub_button)LoadFunction<fun_create_sub_button>("create_sub_button");
            create_sub_button2 = (fun_create_sub_button2)LoadFunction<fun_create_sub_button2>("create_sub_button2");
            create_separator = (fun_create_separator)LoadFunction<fun_create_separator>("create_separator");
            add_button = (fun_add_button)LoadFunction<fun_add_button>("add_button");
            add_separator = (fun_add_separator)LoadFunction<fun_add_separator>("add_separator");

            set_button_text = (fun_set_button_text)LoadFunction<fun_set_button_text>("set_button_text");
            set_button_image = (fun_set_button_image)LoadFunction<fun_set_button_image>("set_button_image");
            set_button_small_image = (fun_set_button_small_image)LoadFunction<fun_set_button_small_image>("set_button_small_image");
            set_button_shortcut = (fun_set_button_shortcut)LoadFunction<fun_set_button_shortcut>("set_button_shortcut");

            set_button_state = (fun_set_button_state)LoadFunction<fun_set_button_state>("set_button_state");
            set_button_down = (fun_set_button_down)LoadFunction<fun_set_button_down>("set_button_down");
            set_button_guid = (fun_set_button_guid)LoadFunction<fun_set_button_guid>("set_button_guid");
            enable_button = (fun_enable_button)LoadFunction<fun_enable_button>("enable_button");
            show_button = (fun_show_button)LoadFunction<fun_show_button>("show_button");

            clicked_button = (fun_clicked_button)LoadFunction<fun_clicked_button>("clicked_button");
            begin_control_group = (fun_begin_control_group)LoadFunction<fun_begin_control_group>("begin_control_group");
            on_context_menu = (fun_on_context_menu)LoadFunction<fun_on_context_menu>("on_context_menu");
            add_context_button = (fun_add_context_button)LoadFunction<fun_add_context_button>("add_context_button");
            add_context_button_with_separator = (fun_add_context_button_with_separator)LoadFunction<fun_add_context_button_with_separator>("add_context_button_with_separator");
            clear_context_menu = (fun_clear_context_menu)LoadFunction<fun_clear_context_menu>("clear_context_menu");

            create_gallery_button = (fun_create_gallery_button)LoadFunction<fun_create_gallery_button>("create_gallery_button");
            set_gallery_style = (fun_set_gallery_style)LoadFunction<fun_set_gallery_style>("set_gallery_style");
            create_gallery_category = (fun_create_gallery_category)LoadFunction<fun_create_gallery_category>("create_gallery_category");
            set_gallery_category_text = (fun_set_gallery_category_text)LoadFunction<fun_set_gallery_category_text>("set_gallery_category_text");
            set_gallery_category_style = (fun_set_gallery_category_style)LoadFunction<fun_set_gallery_category_style>("set_gallery_category_style");
            delete_gallery_category = (fun_delete_gallery_category)LoadFunction<fun_delete_gallery_category>("delete_gallery_category");
            create_gallery_item = (fun_create_gallery_item)LoadFunction<fun_create_gallery_item>("create_gallery_item");
            set_gallery_item_text = (fun_set_gallery_item_text)LoadFunction<fun_set_gallery_item_text>("set_gallery_item_text");
            set_gallery_item_image = (fun_set_gallery_item_image)LoadFunction<fun_set_gallery_item_image>("set_gallery_item_image");
            delete_gallery_item = (fun_delete_gallery_item)LoadFunction<fun_delete_gallery_item>("delete_gallery_item");
            clear_gallery = (fun_clear_gallery)LoadFunction<fun_clear_gallery>("clear_gallery");
            on_gallery_item_context_menu = (fun_on_gallery_item_context_menu)LoadFunction<fun_on_gallery_item_context_menu>("on_gallery_item_context_menu");

            is_center_view = (fun_is_center_view)LoadFunction<fun_is_center_view>("is_center_view");

            get_active_tab = (fun_get_active_tab)LoadFunction<fun_get_active_tab>("get_active_tab");
            on_tab_change = (fun_on_tab_change)LoadFunction<fun_on_tab_change>("on_tab_change");

            get_gui_color = (fun_get_gui_color)LoadFunction<fun_get_gui_color>("get_gui_color");
            get_view_rect = (fun_get_view_rect)LoadFunction<fun_get_view_rect>("get_view_rect");
            show_ifc_structure_grid = (fun_show_ifc_structure_grid)LoadFunction<fun_show_ifc_structure_grid>("show_ifc_structure_grid");
            set_ribbon_state = (fun_set_ribbon_state)LoadFunction<fun_set_ribbon_state>("set_ribbon_state");
            set_grids_state = (fun_set_grids_state)LoadFunction<fun_set_grids_state>("set_grids_state");
            get_grids_state = (fun_get_grids_state)LoadFunction<fun_get_grids_state>("get_grids_state");
            is_touch_mode = (fun_is_touch_mode)LoadFunction<fun_is_touch_mode>("is_touch_mode");

            get_view_state = (fun_get_view_state)LoadFunction<fun_get_view_state>("get_view_state");
            set_view_state = (fun_set_view_state)LoadFunction<fun_set_view_state>("set_view_state");

            render_screenshot = (fun_render_screenshot)LoadFunction<fun_render_screenshot>("render_screenshot");
            set_screenshot_style = (fun_set_screenshot_style)LoadFunction<fun_set_screenshot_style>("set_screenshot_style");

            get_2d_bounds = (fun_get_2d_bounds)LoadFunction<fun_get_2d_bounds>("get_2d_bounds");
            project_to_2d = (fun_project_to_2d)LoadFunction<fun_project_to_2d>("project_to_2d");
            recalc_cross_sections = (fun_recalc_cross_sections)LoadFunction<fun_recalc_cross_sections>("recalc_cross_sections");
            set_cross_sections_style = (fun_set_cross_sections_style)LoadFunction<fun_set_cross_sections_style>("set_cross_sections_style");

            get_volume = (fun_get_volume)LoadFunction<fun_get_volume>("get_volume");
            get_centroid = (fun_get_centroid)LoadFunction<fun_get_centroid>("get_centroid");

            get_direction_camera = (fun_get_direction_camera)LoadFunction<fun_get_direction_camera>("get_direction_camera");
            set_direction_camera = (fun_set_direction_camera)LoadFunction<fun_set_direction_camera>("set_direction_camera");

            set_measure_type = (fun_set_measure_type)LoadFunction<fun_set_measure_type>("set_measure_type");
            get_total_area = (fun_get_total_area)LoadFunction<fun_get_total_area>("get_total_area");

            begin_read_bvf = (fun_begin_read_bvf)LoadFunction<fun_begin_read_bvf>("begin_read_bvf");
            begin_read_bvf_v2 = (fun_begin_read_bvf_v2)LoadFunction<fun_begin_read_bvf_v2>("begin_read_bvf_v2");

            read_bvf = (fun_read_bvf)LoadFunction<fun_read_bvf>("read_bvf");
            read_bvf_v2 = (fun_read_bvf_v2)LoadFunction<fun_read_bvf_v2>("read_bvf_v2");

            save_bvf_sub_file_v2 = (fun_save_bvf_sub_file_v2)LoadFunction<fun_save_bvf_sub_file_v2>("save_bvf_sub_file_v2");
            get_bvf_dir_file_list_v2 = (fun_get_bvf_dir_file_list_v2)LoadFunction<fun_get_bvf_dir_file_list_v2>("get_bvf_dir_file_list_v2");

            get_bvf_size = (fun_get_bvf_size)LoadFunction<fun_get_bvf_size>("get_bvf_size");
            get_bvf_size_v2 = (fun_get_bvf_size_v2)LoadFunction<fun_get_bvf_size_v2>("get_bvf_size_v2");
            on_save_bvf = (fun_on_save_bvf)LoadFunction<fun_on_save_bvf>("on_save_bvf");

            can_clear_model = (fun_can_clear_model)LoadFunction<fun_can_clear_model>("can_clear_model");

            begin_write_bvf = (fun_begin_write_bvf)LoadFunction<fun_begin_write_bvf>("begin_write_bvf");
            begin_write_bvf_v2 = (fun_begin_write_bvf_v2)LoadFunction<fun_begin_write_bvf_v2>("begin_write_bvf_v2");

            write_bvf = (fun_write_bvf)LoadFunction<fun_write_bvf>("write_bvf");
            write_bvf_v2 = (fun_write_bvf_v2)LoadFunction<fun_write_bvf_v2>("write_bvf_v2");
            write_file_to_bvf_v2 = (fun_write_file_to_bvf_v2)LoadFunction<fun_write_file_to_bvf_v2>("write_file_to_bvf_v2");

            file_was_added = (fun_file_was_added)LoadFunction<fun_file_was_added>("file_was_added");
            need_save_bvf = (fun_need_save_bvf)LoadFunction<fun_need_save_bvf>("need_save_bvf");
            save_file = (fun_save_file)LoadFunction<fun_save_file>("save_file");
            need_save_changes = (fun_need_save_changes)LoadFunction<fun_need_save_changes>("need_save_changes");
            on_hover = (fun_on_hover)LoadFunction<fun_on_hover>("on_hover");
            set_hover_objects = (fun_set_hover_objects)LoadFunction<fun_set_hover_objects>("set_hover_objects");
            get_hovered_pos = (fun_get_hovered_pos)LoadFunction<fun_get_hovered_pos>("get_hovered_pos");
            get_dropped_pos = (fun_get_dropped_pos)LoadFunction<fun_get_dropped_pos>("get_dropped_pos");
            get_cutting_planes = (fun_get_cutting_planes)LoadFunction<fun_get_cutting_planes>("get_cutting_planes");
            set_cutting_planes = (fun_set_cutting_planes)LoadFunction<fun_set_cutting_planes>("set_cutting_planes");
            get_active = (fun_get_active)LoadFunction<fun_get_active>("get_active");
            get_current_product = (fun_get_current_product)LoadFunction<fun_get_current_product>("get_current_product");
            has_representation = (fun_has_representation)LoadFunction<fun_has_representation>("has_representation");
            get_bounds = (fun_get_bounds)LoadFunction<fun_get_bounds>("get_bounds");
            get_offset = (fun_get_offset)LoadFunction<fun_get_offset>("get_offset");
            set_offset = (fun_set_offset)LoadFunction<fun_set_offset>("set_offset");
            get_object_materials = (fun_get_object_materials)LoadFunction<fun_get_object_materials>("get_object_materials");
            get_object_materials2 = (fun_get_object_materials2)LoadFunction<fun_get_object_materials2>("get_object_materials2");
            get_object_materials3 = (fun_get_object_materials3)LoadFunction<fun_get_object_materials3>("get_object_materials3");
            get_object_type_style = (fun_get_object_type_style)LoadFunction<fun_get_object_type_style>("get_object_type_style");
            get_object_type_style2 = (fun_get_object_type_style2)LoadFunction<fun_get_object_type_style2>("get_object_type_style2");
            draw_pyramid = (fun_draw_pyramid)LoadFunction<fun_draw_pyramid>("draw_pyramid");

            create_intersection_solid = (fun_create_intersection_solid)LoadFunction<fun_create_intersection_solid>("create_intersection_solid");
            delete_solid = (fun_delete_solid)LoadFunction<fun_delete_solid>("delete_solid");
            draw_solid = (fun_draw_solid)LoadFunction<fun_draw_solid>("draw_solid");
            get_solid_volume = (fun_get_solid_volume)LoadFunction<fun_get_solid_volume>("get_solid_volume");
            get_solid_area = (fun_get_solid_area)LoadFunction<fun_get_solid_area>("get_solid_area");
            get_solid_center_point = (fun_get_solid_center_point)LoadFunction<fun_get_solid_center_point>("get_solid_center_point");
            zoom_to_solids = (fun_zoom_to_solids)LoadFunction<fun_zoom_to_solids>("zoom_to_solids");

            zoom_to_objects = (fun_zoom_to_objects)LoadFunction<fun_zoom_to_objects>("zoom_to_objects");

            get_gui_theme = (fun_get_gui_theme)LoadFunction<fun_get_gui_theme>("get_gui_theme");

            message_box = (fun_message_box)LoadFunction<fun_message_box>("message_box");
            set_open_dialog_params = (fun_set_open_dialog_params)LoadFunction<fun_set_open_dialog_params>("set_open_dialog_params");
            open_file_dialog = (fun_open_file_dialog)LoadFunction<fun_open_file_dialog>("open_file_dialog");
            get_open_dialog_file_count = (fun_get_open_dialog_file_count)LoadFunction<fun_get_open_dialog_file_count>("get_open_dialog_file_count");
            get_open_dialog_file_nr = (fun_get_open_dialog_file_nr)LoadFunction<fun_get_open_dialog_file_nr>("get_open_dialog_file_nr");
            set_save_dialog_params = (fun_set_save_dialog_params)LoadFunction<fun_set_save_dialog_params>("set_save_dialog_params");
            set_save_dialog_default_extension = (fun_set_save_dialog_default_extension)LoadFunction<fun_set_save_dialog_default_extension>("set_save_dialog_default_extension");
            save_dialog = (fun_save_dialog)LoadFunction<fun_save_dialog>("save_dialog");
            get_save_dialog_name = (fun_get_save_dialog_name)LoadFunction<fun_get_save_dialog_name>("get_save_dialog_name");
            show_progress_bar = (fun_show_progress_bar)LoadFunction<fun_show_progress_bar>("show_progress_bar");
            update_progress_bar = (fun_update_progress_bar)LoadFunction<fun_update_progress_bar>("update_progress_bar");

            get_properties2 = (fun_get_properties2)LoadFunction<fun_get_properties2>("get_properties2");
            on_tab_sheet_placement = (fun_on_tab_sheet_placement)LoadFunction<fun_on_tab_sheet_placement>("on_tab_sheet_placement");
            on_tab_sheet_change = (fun_on_tab_sheet_change)LoadFunction<fun_on_tab_sheet_change>("on_tab_sheet_change");

            get_measure_state = (fun_get_measure_state)LoadFunction<fun_get_measure_state>("get_measure_state");
            set_measure_state = (fun_set_measure_state)LoadFunction<fun_set_measure_state>("set_measure_state");

            get_measure_count = (fun_get_measure_count)LoadFunction<fun_get_measure_count>("get_measure_count");
            get_detailed_measure_state = (fun_get_detailed_measure_state)LoadFunction<fun_get_detailed_measure_state>("get_detailed_measure_state");
            set_detailed_measure_state = (fun_set_detailed_measure_state)LoadFunction<fun_set_detailed_measure_state>("set_detailed_measure_state");
            change_measure_state_density = (fun_change_measure_state_density)LoadFunction<fun_change_measure_state_density>("change_measure_state_density");
            construct_measure_state = (fun_construct_measure_state)LoadFunction<fun_construct_measure_state>("construct_measure_state");

            on_object_list_changed = (fun_on_object_list_changed)LoadFunction<fun_on_object_list_changed>("on_object_list_changed");


            get_loaded_ifc_files_count = (fun_get_loaded_ifc_files_count)LoadFunction<fun_get_loaded_ifc_files_count>("get_loaded_ifc_files_count");
            get_loaded_ifc_filename = (fun_get_loaded_ifc_filename)LoadFunction<fun_get_loaded_ifc_filename>("get_loaded_ifc_filename");

            get_plugin_status = (fun_get_plugin_status)LoadFunction<fun_get_plugin_status>("get_plugin_status");
            get_plugin_status2 = (fun_get_plugin_status2)LoadFunction<fun_get_plugin_status2>("get_plugin_status2");
            send_plugin_massage = (fun_send_plugin_massage)LoadFunction<fun_send_plugin_massage>("send_plugin_massage");
            on_plugin_message = (fun_on_plugin_message)LoadFunction<fun_on_plugin_message>("on_plugin_message");

            add_ifc = (fun_add_ifc)LoadFunction<fun_add_ifc>("add_ifc");
            item_command = (fun_item_command)LoadFunction<fun_item_command>("item_command");
            set_item_value = (fun_set_item_value)LoadFunction<fun_set_item_value>("set_item_value");
            get_item_value = (fun_get_item_value)LoadFunction<fun_get_item_value>("get_item_value");

            register_on_button_click = (fun_register_on_button_click)LoadFunction<fun_register_on_button_click>("register_on_button_click");

            create_tab_sheet = (fun_create_tab_sheet)LoadFunction<fun_create_tab_sheet>("create_tab_sheet");
            activate_tab_sheet = (fun_activate_tab_sheet)LoadFunction<fun_activate_tab_sheet>("activate_tab_sheet");
            close_tab_sheet = (fun_close_tab_sheet)LoadFunction<fun_close_tab_sheet>("close_tab_sheet");
            show_tab_sheet = (fun_show_tab_sheet)LoadFunction<fun_show_tab_sheet>("show_tab_sheet");
            is_active_tab_sheet = (fun_is_active_tab_sheet)LoadFunction<fun_is_active_tab_sheet>("is_active_tab_sheet");

            get_projects_offsets = (fun_get_projects_offsets)LoadFunction<fun_get_projects_offsets>("get_projects_offsets");

            bv_zip_get = (fun_bv_zip_get)LoadFunction<fun_bv_zip_get>("bv_zip_get");
            bv_zip_get_file_length = (fun_bv_zip_get_file_length)LoadFunction<fun_bv_zip_get_file_length>("bv_zip_get_file_length");
            bv_zip_read_buffer = (fun_bv_zip_read_buffer)LoadFunction<fun_bv_zip_read_buffer>("bv_zip_read_buffer");
            bv_zip_write_buffer = (fun_bv_zip_write_buffer)LoadFunction<fun_bv_zip_write_buffer>("bv_zip_write_buffer");
        }

        public delegate void callback_fun();
        public delegate void callback_tab_sheet_placement(ref TabSheetPlacement tabSheetPlacement);
        public delegate void callback_tab_sheet_change(ref TabSheetChange tabSheetChange);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool callback_plugin_message([MarshalAs(UnmanagedType.LPWStr)] string dll_from, ref PluginMessage message);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool bool_callback_fun();

        public delegate OBJECT_ID fun_get_selected_id(PLUGIN_ID pid);
        public delegate OBJECT_ID fun_get_all_objects_id(PLUGIN_ID pid);

        public delegate void fun_set_color(PLUGIN_ID pid, OBJECT_ID id, ColorRGB cl, bool transparent);
        public delegate void fun_set_color_object(PLUGIN_ID pid, OBJECT_ID id, ColorRGB cl, bool transparent);
        public delegate void fun_set_default_color(PLUGIN_ID pid, OBJECT_ID id);
        public delegate void fun_use_selection_color(PLUGIN_ID pid, bool b);

        public delegate void fun_treat_openings_as_normal_objects(PLUGIN_ID pid, bool b);
        public delegate void fun_set_drag_mode(PLUGIN_ID pid, bool b);

        public delegate void fun_set_visible(PLUGIN_ID pid, OBJECT_ID id, int visible_type);
        public delegate void fun_set_visible_object(PLUGIN_ID pid, OBJECT_ID id, int visible_type);
        public delegate void fun_set_visible_many(PLUGIN_ID pid, OBJECT_ID[] id, uint count, int visible_type);
        public delegate void fun_set_visible_many_objects(PLUGIN_ID pid, OBJECT_ID[] id, uint count, int visible_type);

        public delegate void fun_set_object_color(PLUGIN_ID pid, OBJECT_ID id, Color cl, bool with_all_children);
        public delegate void fun_set_object_color_rgb(PLUGIN_ID pid, OBJECT_ID id, ColorRGB cl, bool with_all_children);
        public delegate void fun_set_object_default_color(PLUGIN_ID pid, OBJECT_ID id, bool with_all_children);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_get_object_color(PLUGIN_ID pid, OBJECT_ID id, ref Color cl);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_get_object_deafult_color(PLUGIN_ID pid, OBJECT_ID id, ref Color cl, int index);

        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_retrieve_color(PLUGIN_ID pid, OBJECT_ID id, ref Color color, ref int color_options, ColorSource color_source, RetrieveColorOption retrieve_color_option);
        public delegate uint fun_assign_color(PLUGIN_ID pid, OBJECT_ID id, ref Color color, int color_options, int color_targets, int assign_color_options);
        public delegate uint fun_deactivate_color(PLUGIN_ID pid, OBJECT_ID id, int color_targets, int deactivate_color_options);

        [return: MarshalAs(UnmanagedType.I1)]
        public delegate char fun_get_transparency(PLUGIN_ID pid);
        public delegate void fun_add_color_to_transparency_list(PLUGIN_ID pid, OBJECT_ID id, ref Color cl);
        public delegate void fun_set_object_edge_color(PLUGIN_ID pid, OBJECT_ID id, Color cl, bool with_all_children);
        public delegate void fun_set_object_edge_default_color(PLUGIN_ID pid, OBJECT_ID id, bool with_all_children);

        public delegate void fun_set_object_visible(PLUGIN_ID pid, OBJECT_ID id, int visible_type, bool with_all_children);
        public delegate void fun_set_object_active(PLUGIN_ID pid, OBJECT_ID id, bool active, bool with_all_children);

        public delegate void fun_select(PLUGIN_ID pid, OBJECT_ID id, bool b);
        public delegate void fun_unselect_openings(PLUGIN_ID pid, OBJECT_ID id);
        public delegate void fun_select_many(PLUGIN_ID pid, OBJECT_ID[] id, uint count, int type);
        public delegate void fun_apply_select_rules(PLUGIN_ID pid);
        public delegate void fun_close_application(PLUGIN_ID pid);

        public delegate void fun_zoom_to(PLUGIN_ID pid, OBJECT_ID id);
        public delegate void fun_zoom_to_in_view(PLUGIN_ID pid, OBJECT_ID id, int width, int height);
        public delegate void fun_zoom_to_box(PLUGIN_ID pid, ref RelativePos a, ref RelativePos b);
        public delegate void fun_get_camera_pos(PLUGIN_ID pid, ref CameraPos pos);
        public delegate void fun_set_camera_pos(PLUGIN_ID pid, ref CameraPos pos);
        public delegate void fun_invalidate(PLUGIN_ID pid);
        public delegate float fun_get_draw_time(PLUGIN_ID pid);

        public delegate uint fun_get_all_objects(PLUGIN_ID pid, [Out] OBJECT_ID[] buf, uint count);
        public delegate uint fun_get_selected(PLUGIN_ID pid, [Out] OBJECT_ID[] buf, uint count);
        public delegate uint fun_get_visible(PLUGIN_ID pid, [Out] OBJECT_ID[] buf, uint count);
        public delegate void fun_get_object_state(PLUGIN_ID pid, [In] OBJECT_ID[] id, [Out] ObjectState[] state, uint count);

        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_is_visible(PLUGIN_ID pid, OBJECT_ID id);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_is_transparent(PLUGIN_ID pid, OBJECT_ID id);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_is_selected(PLUGIN_ID pid, OBJECT_ID id);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_is_active(PLUGIN_ID pid, OBJECT_ID id);

        public delegate uint fun_get_childs(PLUGIN_ID pid, OBJECT_ID id, [Out] OBJECT_ID[] buf, uint count);
        public delegate void fun_get_object_info(PLUGIN_ID pid, OBJECT_ID[] id, [Out] ObjectInfo_[] info, uint count);

        public delegate void fun_set_user_data(PLUGIN_ID pid, OBJECT_ID id, IntPtr user_data, bool with_all_children);
        public delegate IntPtr fun_get_user_data(PLUGIN_ID pid, OBJECT_ID id);
        public delegate OBJECT_ID fun_get_project(PLUGIN_ID pid, OBJECT_ID id);
        public delegate OBJECT_ID fun_get_building(PLUGIN_ID pid, OBJECT_ID id);
        public delegate OBJECT_ID fun_get_storey(PLUGIN_ID pid, OBJECT_ID id);
        public delegate OBJECT_ID fun_get_parent(PLUGIN_ID pid, OBJECT_ID id);
        public delegate void fun_get_object_info2(PLUGIN_ID pid, OBJECT_ID[] id, [Out] ObjectInfo2_[] info2, uint count);

        public delegate uint fun_get_layers(PLUGIN_ID pid, [Out] Layer_[] buf, uint count);
        public delegate uint fun_get_layer_objects(PLUGIN_ID pid, LAYER_REF id, [Out] OBJECT_ID[] buf, uint count);
        public delegate uint fun_get_zones(PLUGIN_ID pid, [Out] Zone_[] buf, uint count);
        public delegate uint fun_get_zones2(PLUGIN_ID pid, [Out] Zone2_[] buf, uint count);
        public delegate uint fun_get_zone_objects(PLUGIN_ID pid, ZONE_REF id, [Out] OBJECT_ID[] buf, uint count);

        public delegate uint fun_get_systems(PLUGIN_ID pid, [Out] System_[] buf, uint count);
        public delegate uint fun_get_system_objects(PLUGIN_ID pid, SYSTEM_REF system_ref, [Out] OBJECT_ID[] buf, uint count);

        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_delete_object_property_or_set(PLUGIN_ID pid, ref PropertyDef buf);

        public delegate void fun_add_or_change_object_property(PLUGIN_ID pid, ref PropertyDef propertyDef, ref PropertyData propertyData);



        public delegate uint fun_get_groups(PLUGIN_ID pid, [Out] Group_[] buf, uint count);
        public delegate uint fun_get_group_objects(PLUGIN_ID pid, GROUP_REF id, [Out] OBJECT_ID[] buf, uint count);

        public delegate uint fun_get_property_sets(PLUGIN_ID pid, OBJECT_ID id, [Out] PropertySet_[] buf, uint count);
        public delegate uint fun_get_properties(PLUGIN_ID pid, ref PropertyGet pg, [Out] Property_[] buf, uint count);
        public delegate uint fun_filter_properties(PLUGIN_ID pid, ref PropertyFilter pf, [Out] Property_[] buf, uint count);
        public delegate void fun_select_property(PLUGIN_ID pid, int set_nr, int property_nr, bool select_value);

        public delegate uint fun_get_object_properties(PLUGIN_ID pid, OBJECT_ID id, int flag, [Out] PropertySetData_[] buf, uint count);
        public delegate uint fun_get_object_properties2(PLUGIN_ID pid, OBJECT_ID id, int flag, [Out] PropertySetData2_[] buf, uint count);
        public delegate uint fun_get_material_properties(PLUGIN_ID pid, MATERIAL_REF material_ref, int flag, [Out] PropertySetData_[] buf, uint count);
        public delegate uint fun_get_type_style_properties(PLUGIN_ID pid, OBJECT_TYPE_STYLE_REF type_style_ref, int flag, [Out] PropertySetData_[] buf, uint count);

        public delegate uint fun_get_object_classification_references(PLUGIN_ID pid, OBJECT_ID id, [Out] ClassificationReference_[] buf, uint count);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_get_classification_data(PLUGIN_ID pid, REF cl, ref Classification_ classification);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_get_classification_reference_data(PLUGIN_ID pid, REF cl_ref, ref ClassificationReference_ classification_reference);

        public delegate uint fun_get_classifications(PLUGIN_ID pid, [Out] Classification_[] buf, uint count);
        public delegate uint fun_get_classification_references(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string classification_name, [Out] ClassificationReference_[] references, uint count);

        public delegate double fun_get_unit_factor(PLUGIN_ID pid);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_first_geometry(PLUGIN_ID pid, OBJECT_ID id);
        public delegate void fun_get_total_geometry_bounds(PLUGIN_ID pid, ref Bounds bounds);
        public delegate uint fun_get_geometry(PLUGIN_ID pid, [Out] Face[] faces, uint count);
        public delegate void fun_get_geometry_color(PLUGIN_ID pid, ref Color face_color, ref Color line_color);
        public delegate uint fun_get_geometry_edges(PLUGIN_ID pid, [Out] Edge[] Edges, uint count);
        public delegate OBJECT_ID fun_create_user_object(PLUGIN_ID pid, OBJECT_ID parent, [MarshalAs(UnmanagedType.LPStr)] string ifc_entity_name, [MarshalAs(UnmanagedType.LPWStr)] string name, [MarshalAs(UnmanagedType.LPWStr)] string descrption);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_check_triangle(PLUGIN_ID pid, Face face);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_delete_user_object(PLUGIN_ID pid, OBJECT_ID id);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_add_geometry_user_object(PLUGIN_ID pid, OBJECT_ID id, Face[] faces, uint count, Color[] cl);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_delete_geometry_user_object(PLUGIN_ID pid, OBJECT_ID id);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_set_geometry_edges_user_object(PLUGIN_ID pid, OBJECT_ID id, Edge[] Edges, uint count);

        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_next_geometry(PLUGIN_ID pid);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_is_online_licence(PLUGIN_ID pid);

        public delegate void fun_on_property_change(PLUGIN_ID pid, IntPtr fun);

        public delegate uint fun_get_object_corners(PLUGIN_ID pid, OBJECT_ID id, [Out] Vector3d[] cornersVector, uint cornersCount);
        public delegate uint fun_get_object_edges(PLUGIN_ID pid, OBJECT_ID id, [Out] Edge[] edges, uint edgeCount);
        public delegate bool fun_get_object_area(PLUGIN_ID pid, OBJECT_ID id, ref MeasuredArea measure);

        public delegate void fun_on_measure_change(PLUGIN_ID pid, IntPtr fun);
        public delegate void fun_get_measure(PLUGIN_ID pid, ref Measure measure);
        public delegate void fun_get_measure_v1(PLUGIN_ID pid, ref MeasureV1 measure);
        public delegate uint fun_get_measure_objects(PLUGIN_ID pid, [Out] OBJECT_ID[] buf, uint count);
        public delegate uint fun_get_measure_elements(PLUGIN_ID pid, [Out] double[] buf, uint count);
        public delegate void fun_clear_measure(PLUGIN_ID pid);

        public delegate IntPtr fun_get_loaded_ifc_path(PLUGIN_ID pid);
        public delegate IntPtr fun_get_project_path(PLUGIN_ID pid, OBJECT_ID id);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_extract_file_from_bvf(PLUGIN_ID pid, OBJECT_ID project_id, [MarshalAs(UnmanagedType.LPWStr)] string to_ifc_file_name);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_load_ifc(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string path);
        public delegate bool fun_load_ifc_from_buffer(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string file_name, byte[] buffer, uint buf_size);
        public delegate OBJECT_ID fun_calculate_object_id(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPStr)] string ifc_id);

        public delegate void fun_on_model_load(PLUGIN_ID pid, IntPtr fun);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_on_model_load_v2(bool is_bvf_file);
        public delegate void fun_on_model_save_v2();
        public delegate void fun_on_model_clear(PLUGIN_ID pid, IntPtr fun);
        public delegate void fun_on_main_form_close(PLUGIN_ID pid, IntPtr fun);
        public delegate void fun_on_selection_change(PLUGIN_ID pid, IntPtr fun);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_get_clicked_pos(PLUGIN_ID pid, ref RelativePos pos);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_get_clicked_normal(PLUGIN_ID pid, ref Vector3d normal);
        public delegate void fun_on_draw(PLUGIN_ID pid, IntPtr fun);
        public delegate void fun_set_draw_object_id(PLUGIN_ID pid, uint id);
        public delegate void fun_on_draw_object_click(PLUGIN_ID pid, IntPtr fun);
        public delegate void fun_on_undo_redo_action(PLUGIN_ID pid, IntPtr fun);

        public delegate uint fun_get_draw_object_id(PLUGIN_ID pid);

        public delegate void fun_draw_point(PLUGIN_ID pid, ref Point point);
        public delegate void fun_draw_line(PLUGIN_ID pid, ref Line line);
        public delegate void fun_draw_absolute_line(PLUGIN_ID pid, ref AbsoluteLine line);
        public delegate int fun_draw_image(PLUGIN_ID pid, ref Image image);
        public delegate int fun_draw_absolute_image(PLUGIN_ID pid, ref AbsoluteImage absoluteImage);
        public delegate int fun_draw_image_file(PLUGIN_ID pid, ref ImageFile imageFile);
        public delegate int fun_draw_absolute_image_file(PLUGIN_ID pid, ref AbsoluteImageFile absoluteImageFile);
        public delegate void fun_draw_sphere(PLUGIN_ID pid, ref Sphere sphere);

        public delegate void fun_load_icon_image(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string image_path);
        public delegate void fun_draw_icon(PLUGIN_ID pid, ref Icon icon);

        public delegate void fun_draw_label(PLUGIN_ID pid, ref Label label);

        public delegate void fun_get_element_pos(PLUGIN_ID pid, ref RelativePos pos);

        public delegate bool fun_save_file_as(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string file_name, int save_type);

        public delegate IntPtr fun_get_language(PLUGIN_ID pid);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_load_texts(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string file_name);
        public delegate IntPtr fun_get_text(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPStr)] string txt_id);
        public delegate IntPtr fun_get_text_global(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPStr)] string txt_id);

        public delegate IntPtr fun_get_entity_type_name(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPStr)] string ifc_entity_name);

        public delegate OBJECT_ID fun_get_object_below_mouse(PLUGIN_ID pid, int x, int y);

        public delegate int fun_register_undo_action(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string action_name);

        public delegate int fun_create_tab(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string name);
        public delegate int fun_create_group(PLUGIN_ID pid, int tab_id, [MarshalAs(UnmanagedType.LPWStr)] string name);
        public delegate void fun_show_group(PLUGIN_ID pid, int group_id, bool visible);

        public delegate int fun_create_button(PLUGIN_ID pid, int group_id, IntPtr fun);
        public delegate int fun_create_small_button(PLUGIN_ID pid, int group_id, IntPtr fun);
        public delegate int fun_create_checkbox(PLUGIN_ID pid, int group_id, IntPtr fun);
        public delegate int fun_create_radio_button(PLUGIN_ID pid, int group_id, IntPtr fun);
        public delegate int fun_create_dropdown_button(PLUGIN_ID pid, int group_id, IntPtr fun);
        public delegate int fun_create_sub_button(PLUGIN_ID pid, int group_id, bool small);
        public delegate int fun_create_sub_button2(PLUGIN_ID pid, int group_id, bool small, IntPtr fun);
        public delegate int fun_create_separator(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string caption);
        public delegate int fun_add_button(PLUGIN_ID pid, int parent_button_id, int button_id);
        public delegate void fun_add_separator(PLUGIN_ID pid, int parent_button_id, [MarshalAs(UnmanagedType.LPWStr)] string caption);

        public delegate void fun_set_button_text(PLUGIN_ID pid, int button_id, [MarshalAs(UnmanagedType.LPWStr)] string caption, [MarshalAs(UnmanagedType.LPWStr)] string hint);
        public delegate void fun_set_button_image(PLUGIN_ID pid, int button_id, [MarshalAs(UnmanagedType.LPWStr)] string large_img_path);
        public delegate void fun_set_button_small_image(PLUGIN_ID pid, int button_id, [MarshalAs(UnmanagedType.LPWStr)] string small_img_path);
        public delegate void fun_set_button_shortcut(PLUGIN_ID pid, int button_id, [MarshalAs(UnmanagedType.LPWStr)] string shortcut);

        public delegate void fun_set_button_state(PLUGIN_ID pid, int button_id, bool enabled, bool down);
        public delegate void fun_set_button_down(PLUGIN_ID pid, int button_id, bool down);
        public delegate void fun_set_button_guid(PLUGIN_ID pid, int button_id, [MarshalAs(UnmanagedType.LPStr)] string str);
        public delegate void fun_enable_button(PLUGIN_ID pid, int button_id, bool enable);
        public delegate void fun_show_button(PLUGIN_ID pid, int button_id, bool show);

        public delegate int fun_clicked_button(PLUGIN_ID pid);
        public delegate void fun_begin_control_group(PLUGIN_ID pid);
        public delegate void fun_on_context_menu(PLUGIN_ID pid, IntPtr fun);
        public delegate void fun_add_context_button(PLUGIN_ID pid, int button_id);
        public delegate void fun_add_context_button_with_separator(PLUGIN_ID pid, int button_id);
        public delegate void fun_clear_context_menu(PLUGIN_ID pid);

        public delegate int fun_create_gallery_button(PLUGIN_ID pid, int group_id, bool small, callback_fun fun);
        public delegate void fun_set_gallery_style(PLUGIN_ID pid, int gallery_id, int min_col_count, int style);
        public delegate int fun_create_gallery_category(PLUGIN_ID pid, int gallery_id);
        public delegate void fun_set_gallery_category_text(PLUGIN_ID pid, int gallery_category_id, [MarshalAs(UnmanagedType.LPWStr)] string caption);
        public delegate void fun_set_gallery_category_style(PLUGIN_ID pid, int gallery_category_id, int display_texts, int texts_posistion);
        public delegate void fun_delete_gallery_category(PLUGIN_ID pid, int gallery_category_id);
        public delegate int fun_create_gallery_item(PLUGIN_ID pid, int gallery_category_id, callback_fun fun);
        public delegate void fun_set_gallery_item_text(PLUGIN_ID pid, int gallery_item_id, [MarshalAs(UnmanagedType.LPWStr)] string caption, [MarshalAs(UnmanagedType.LPWStr)] string descrption);
        public delegate void fun_set_gallery_item_image(PLUGIN_ID pid, int gallery_item_id, [MarshalAs(UnmanagedType.LPWStr)] string path);
        public delegate void fun_delete_gallery_item(PLUGIN_ID pid, int gallery_item_id);
        public delegate void fun_clear_gallery(PLUGIN_ID pid, int gallery_id);
        public delegate void fun_on_gallery_item_context_menu(PLUGIN_ID pid, callback_fun fun);

        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_is_center_view(PLUGIN_ID pid);

        public delegate int fun_get_active_tab(PLUGIN_ID pid);
        public delegate void fun_on_tab_change(PLUGIN_ID pid, IntPtr fun);

        public delegate ColorRGB fun_get_gui_color(PLUGIN_ID pid, int gui_color_id);
        public delegate void fun_get_view_rect(PLUGIN_ID pid, ref Rect rect);
        public delegate void fun_show_ifc_structure_grid(PLUGIN_ID pid, bool force_show_properties);
        public delegate void fun_set_ribbon_state(PLUGIN_ID pid, int state);
        public delegate void fun_set_grids_state(PLUGIN_ID pid, int state);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_get_grids_state(PLUGIN_ID pid);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_is_touch_mode(PLUGIN_ID pid);

        public delegate uint fun_get_view_state(PLUGIN_ID pid, int flags, byte[] buf, uint buf_size);
        public delegate void fun_set_view_state(PLUGIN_ID pid, int flags, byte[] buf, uint buf_size);

        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_render_screenshot(PLUGIN_ID pid, uint w, uint h, byte[] buf);
        public delegate void fun_set_screenshot_style(PLUGIN_ID pid, int style);

        public delegate void fun_get_2d_bounds(PLUGIN_ID pid, OBJECT_ID[] id, [Out] Bounds[] bounds, uint count, int flags);
        public delegate void fun_project_to_2d(PLUGIN_ID pid, RelativePos[] pos, [Out] Vector3d[] pos_2d, uint count, int flags);
        public delegate void fun_recalc_cross_sections(PLUGIN_ID pid, int flags);
        public delegate void fun_set_cross_sections_style(PLUGIN_ID pid, int style, ColorRGB cl, bool bold);

        /// ////
        public delegate double fun_get_volume(PLUGIN_ID pid, OBJECT_ID id);
        public delegate void fun_get_centroid(PLUGIN_ID pid, OBJECT_ID id, out Vector3d offset, out double weight);

        public delegate void fun_get_direction_camera(PLUGIN_ID pid, ref DirectionCamera cam);
        public delegate void fun_set_direction_camera(PLUGIN_ID pid, ref DirectionCamera cam);

        public delegate void fun_set_measure_type(PLUGIN_ID pid, int type, int param);
        public delegate double fun_get_total_area(PLUGIN_ID pid, OBJECT_ID id);

        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_begin_read_bvf(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string name);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_begin_read_bvf_v2(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string name);

        public delegate uint fun_read_bvf(PLUGIN_ID pid, byte[] buf, uint buf_size);
        public delegate uint fun_read_bvf_v2(PLUGIN_ID pid, byte[] buf, uint buf_size);

        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_save_bvf_sub_file_v2(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string saveAsFileName);
        public delegate string[] fun_get_bvf_dir_file_list_v2(PLUGIN_ID pid);

        public delegate uint fun_get_bvf_size(PLUGIN_ID pid);
        public delegate uint fun_get_bvf_size_v2(PLUGIN_ID pid);

        public delegate void fun_on_save_bvf(PLUGIN_ID pid, IntPtr fun);

        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_begin_write_bvf(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string name);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_begin_write_bvf_v2(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string name);


        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_write_bvf(PLUGIN_ID pid, byte[] buf, uint buf_size);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_write_bvf_v2(PLUGIN_ID pid, byte[] buf, uint buf_size);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_write_file_to_bvf_v2(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string name);

        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_file_was_added(PLUGIN_ID pid);
        public delegate void fun_need_save_bvf(PLUGIN_ID pid, bool flag);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_save_file(PLUGIN_ID pid);
        public delegate void fun_need_save_changes(PLUGIN_ID pid, bool flag);
        public delegate void fun_on_hover(PLUGIN_ID pid, IntPtr fun);
        public delegate void fun_set_hover_objects(PLUGIN_ID pid, OBJECT_ID[] id, uint count);
        public delegate void fun_get_hovered_pos(PLUGIN_ID pid, ref RelativePos pos);
        public delegate void fun_get_dropped_pos(PLUGIN_ID pid, ref RelativePos pos);

        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_can_clear_model(PLUGIN_ID pid, IntPtr fun);

        public delegate uint fun_get_cutting_planes(PLUGIN_ID pid, [Out] CuttingPlane[] buf, uint count);
        public delegate void fun_set_cutting_planes(PLUGIN_ID pid, CuttingPlane[] buf, uint count);
        public delegate uint fun_get_active(PLUGIN_ID pid, [Out] OBJECT_ID[] buff, uint count);
        public delegate OBJECT_ID fun_get_current_product(PLUGIN_ID pid);

        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_has_representation(PLUGIN_ID pid, OBJECT_ID id);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_get_bounds(PLUGIN_ID pid, OBJECT_ID id, ref Bounds bounds);
        public delegate bool fun_get_offset(PLUGIN_ID pid, OBJECT_ID id, ref Vector3d offset);
        public delegate bool fun_set_offset(PLUGIN_ID pid, OBJECT_ID id, ref Vector3d offset);
        public delegate uint fun_get_object_materials(PLUGIN_ID pid, OBJECT_ID id, [Out] Material_[] buf, uint count);
        public delegate uint fun_get_object_materials2(PLUGIN_ID pid, OBJECT_ID id, [Out] Material2_[] buf, uint count);
        public delegate uint fun_get_object_materials3(PLUGIN_ID pid, OBJECT_ID id, [Out] Material3_[] buf, uint count);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_get_object_type_style(PLUGIN_ID pid, OBJECT_ID id, ref ObjectTypeStyle_ result);
        public delegate bool fun_get_object_type_style2(PLUGIN_ID pid, OBJECT_ID id, ref ObjectTypeStyle2_ result);

        public delegate void fun_draw_pyramid(PLUGIN_ID pid, ref Pyramid pyramid);

        public delegate SOLID_ID fun_create_intersection_solid(PLUGIN_ID pid, OBJECT_ID object_a, OBJECT_ID object_b);
        public delegate void fun_delete_solid(PLUGIN_ID pid, SOLID_ID solid_id);
        public delegate void fun_draw_solid(PLUGIN_ID pid, SOLID_ID solid_id, Color color);
        public delegate double fun_get_solid_volume(PLUGIN_ID pid, SOLID_ID solid_id);
        public delegate double fun_get_solid_area(PLUGIN_ID pid, SOLID_ID solid_id);
        public delegate void fun_get_solid_center_point(PLUGIN_ID pid, SOLID_ID solid_id, ref Vertex center);
        public delegate void fun_zoom_to_solids(PLUGIN_ID pid, SOLID_ID[] id, uint count);

        public delegate void fun_zoom_to_objects(PLUGIN_ID pid, OBJECT_ID[] id, uint count);
        public delegate int fun_get_gui_theme(PLUGIN_ID pid);
        public delegate int fun_message_box(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string caption, [MarshalAs(UnmanagedType.LPWStr)] string message, int flags);

        public delegate void fun_set_open_dialog_params(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string filter, [MarshalAs(UnmanagedType.LPWStr)] string directory);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_open_file_dialog(PLUGIN_ID pid, bool multiple_files);
        public delegate uint fun_get_open_dialog_file_count(PLUGIN_ID pid);
        public delegate IntPtr fun_get_open_dialog_file_nr(PLUGIN_ID pid, uint nr);
        public delegate void fun_set_save_dialog_params(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string filter, [MarshalAs(UnmanagedType.LPWStr)] string directory, [MarshalAs(UnmanagedType.LPWStr)] string file_name);
        public delegate void fun_set_save_dialog_default_extension(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string extension);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_save_dialog(PLUGIN_ID pid, bool select_directory_only);
        public delegate IntPtr fun_get_save_dialog_name(PLUGIN_ID pid);
        public delegate void fun_show_progress_bar(PLUGIN_ID pid, bool show, int style, [MarshalAs(UnmanagedType.LPWStr)] string title);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_update_progress_bar(PLUGIN_ID pid, int percent, [MarshalAs(UnmanagedType.LPWStr)] string message, int second_percent, [MarshalAs(UnmanagedType.LPWStr)] string second_message);

        public delegate uint fun_get_properties2(PLUGIN_ID pid, ref PropertyGet pg, [Out] Property2_[] buf, uint count);
        public delegate void fun_on_tab_sheet_placement(PLUGIN_ID pid, IntPtr fun);
        public delegate void fun_on_tab_sheet_change(PLUGIN_ID pid, IntPtr fun);

        public delegate uint fun_get_measure_state(PLUGIN_ID pid, byte[] buf, uint buf_size);
        public delegate void fun_set_measure_state(PLUGIN_ID pid, int flags, byte[] buf, uint buf_size);

        public delegate uint fun_get_measure_count(PLUGIN_ID pid);
        public delegate uint fun_get_detailed_measure_state(PLUGIN_ID pid, uint measure_nr, byte[] buf, uint buf_size, ref MeasureDetail details);
        public delegate void fun_set_detailed_measure_state(PLUGIN_ID pid, int flags, byte[] state, uint size);
        public delegate void fun_change_measure_state_density(PLUGIN_ID pid, byte[] state, double density);
        public delegate uint fun_construct_measure_state(PLUGIN_ID pid, [Out] ConstructMeasureState_[] construct, byte[] buf, uint size);

        public delegate void fun_on_object_list_changed(PLUGIN_ID pid, IntPtr fun);

        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_add_ifc(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string path);
        public delegate uint fun_get_loaded_ifc_files_count(PLUGIN_ID pid);
        public delegate IntPtr fun_get_loaded_ifc_filename(PLUGIN_ID pid, uint file_index);

        public delegate void fun_get_plugin_status(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string dll_name, ref PluginStatus status);
        public delegate void fun_get_plugin_status2(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string dll_name, ref PluginStatus2 status);

        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_send_plugin_massage(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string dll_name, ref PluginMessage message);
        public delegate void fun_on_plugin_message(PLUGIN_ID pid, IntPtr fun);

        public delegate int fun_create_tab_sheet(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string name, bool allow_close);
        public delegate void fun_activate_tab_sheet(PLUGIN_ID pid, int id);
        public delegate void fun_close_tab_sheet(PLUGIN_ID pid, int id);
        public delegate void fun_show_tab_sheet(PLUGIN_ID pid, int id, bool show);
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_is_active_tab_sheet(PLUGIN_ID pid, int id);

        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool fun_item_command(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string item_name, [MarshalAs(UnmanagedType.LPWStr)] string command);
        public delegate void fun_set_item_value(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string item_name, double value);
        public delegate double fun_get_item_value(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string item_name, [MarshalAs(UnmanagedType.LPWStr)] string value_type);

        public delegate double fun_register_on_button_click(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string item_name, IntPtr fun);

        public delegate int fun_get_projects_offsets(PLUGIN_ID pid, [Out] ProjectOffset[] offsets, int count);

        public delegate void fun_bv_zip_add(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string zip_file_name, [MarshalAs(UnmanagedType.LPWStr)] string add_file_name);
        public delegate bool fun_bv_zip_get(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string zip_file_name, [MarshalAs(UnmanagedType.LPWStr)] string file_name, [MarshalAs(UnmanagedType.LPWStr)] string bv_zip_get);
        public delegate uint fun_bv_zip_get_file_length(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string zip_file_name, [MarshalAs(UnmanagedType.LPWStr)] string file_name);
        public delegate uint fun_bv_zip_read_buffer(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string zip_file_name, [MarshalAs(UnmanagedType.LPWStr)] string file_name, byte[] buffer, uint buffer_length);
        public delegate bool fun_bv_zip_write_buffer(PLUGIN_ID pid, [MarshalAs(UnmanagedType.LPWStr)] string zip_file_name, [MarshalAs(UnmanagedType.LPWStr)] string file_name, out byte[] buffer, uint buffer_length);

        public static String exeName;
        private static IntPtr moduleHandle;

        static Api()
        {
            moduleHandle = GetModuleHandle(null);

            var fileName = new StringBuilder(1024);
            GetModuleFileName(IntPtr.Zero, fileName, fileName.Capacity);
            exeName = fileName.ToString();

            loadPluginFunctions();
        }

        public static Delegate LoadFunction<T>(string functionName)
        {
            try
            {
                var functionAddress = GetProcAddress(moduleHandle, functionName);
                return Marshal.GetDelegateForFunctionPointer(functionAddress, typeof(T));
            }
            catch
            {
                return null;
            }
        }

        [DllImport("Kernel32.dll")]
        private static extern IntPtr LoadLibrary(string path);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("Kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll")]
        private static extern uint GetModuleFileName(IntPtr hModule, StringBuilder lpFilename, int nSize);
    }

    #endregion


    #region High level API

    /// <summary>
    /// Use this class to acces API
    /// </summary>
    public class ApiWrapper
    {
        private uint GetLenth<T>(T[] t)
        {
            return (uint)(t?.Length ?? 0);
        }

        public ApiWrapper(PLUGIN_ID plugin_id)
        {
            pid = plugin_id;
        }

        public String GetViewerDirectory()
        {
            return Path.GetDirectoryName(Api.exeName);
        }

        public String GetDLLDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public OBJECT_ID GetSelectedId()
        {
            return Api.get_selected_id(pid);
        }

        public OBJECT_ID GetAllObjectsId()
        {
            return Api.get_all_objects_id(pid);
        }

        public void UseSelectionColor(bool b)
        {
            Api.use_selection_color(pid, b);
        }

        public void TreatOpeningsAsNormalObjects(bool b)
        {
            Api.treat_openings_as_normal_objects(pid, b);
        }

        public void SetDragMode(bool b)
        {
            Api.set_drag_mode(pid, b);
        }

        public void SetObjectColor(OBJECT_ID id, Color cl, bool with_all_children)
        {
            Api.set_object_color(pid, id, cl, with_all_children);
        }

        public void SetObjectColorRGB(OBJECT_ID id, ColorRGB cl, bool with_all_children)
        {
            Api.set_object_color_rgb(pid, id, cl, with_all_children);
        }

        public void SetObjectDefaultColor(OBJECT_ID id, bool with_all_children)
        {
            Api.set_object_default_color(pid, id, with_all_children);
        }

        public bool GetObjectColor(OBJECT_ID id, ref Color cl)
        {
            return Api.get_object_color(pid, id, ref cl);
        }
        public bool GetObjectDeafultColor(OBJECT_ID id, ref Color cl, int index)
        {
            return Api.get_object_deafult_color(pid, id, ref cl, index);
        }

        public bool RetrieveColor(OBJECT_ID id, ref Color color, ref int colorOptions, ColorSource colorSource = ColorSource.cs_face, RetrieveColorOption retrieveColorOption = RetrieveColorOption.rco_current)
        {
            return Api.retrieve_color(pid, id, ref color, ref colorOptions, colorSource, retrieveColorOption);
        }

        enum ColorTarget
        {
            ct_face = 1, // color is assigned to object's faces
            ct_edge = 2, // color is assigned to object's edges
        };

        // options defining what kind of additional actions to perform upon assigning new color
        enum AssignColorOption
        {
            aco_none = 0,                 // no additional actions
            aco_with_children = 1,        // affect also all target object's children
            aco_ignore_color = 2,         // 'color' parameter is ignored when assigning new color and color options
            aco_ignore_color_options = 4, // 'color_options' parameter is ignored when assigning new color and color options
        };

        public uint AssignColor(PLUGIN_ID pid, OBJECT_ID id, ref Color color, int color_options = (int)ColorOption.co_none, int color_targets = (int)ColorTarget.ct_face, int assign_color_options = (int)AssignColorOption.aco_none)
        {
            return Api.assign_color(pid, id, ref color, color_options, color_targets, assign_color_options);
        }

        public uint DeactivateColor(PLUGIN_ID pid, OBJECT_ID id, int color_targets = (int)ColorTarget.ct_face, int deactivate_color_options = (int)DeactivateColorOption.dco_none)
        {
            return Api.deactivate_color(pid, id, color_targets, deactivate_color_options);
        }

        public char GetTransparency()
        {
            return Api.get_transparency(pid);
        }
        public void AddColorToTransparencyList(OBJECT_ID id, ref Color cl)
        {
            Api.add_color_to_transparency_list(pid, id, ref cl);
        }

        public void SetObjectEdgeColor(OBJECT_ID id, Color cl, bool with_all_children)
        {
            Api.set_object_edge_color(pid, id, cl, with_all_children);
        }

        public void SetObjectEdgeDefaultColor(OBJECT_ID id, bool with_all_children)
        {
            Api.set_object_edge_default_color(pid, id, with_all_children);
        }

        public void SetObjectVisible(OBJECT_ID id, int visible_type, bool with_all_children)
        {
            Api.set_object_visible(pid, id, visible_type, with_all_children);
        }

        public void SetObjectActive(OBJECT_ID id, bool active, bool with_all_children)
        {
            Api.set_object_active(pid, id, active, with_all_children);
        }

        public void Select(OBJECT_ID id, bool b)
        {
            Api.select(pid, id, b);
        }

        public void UnselectOpenings(OBJECT_ID id)
        {
            Api.unselect_openings(pid, id);
        }

        public void SelectMany(OBJECT_ID[] id, SelectType type)
        {
            Api.select_many(pid, id, GetLenth(id), (int)type);
        }

        public void ApplySelectRules()
        {
            Api.apply_select_rules(pid);
        }

        public void CloseApplication()
        {
            Api.close_application(pid);
        }

        public void ZoomTo(OBJECT_ID id)
        {
            Api.zoom_to(pid, id);
        }

        public void ZoomToInView(OBJECT_ID id, int width, int height)
        {
            Api.zoom_to_in_view(pid, id, width, height);
        }

        public void ZoomToBox(OBJECT_ID id, RelativePos a, RelativePos b)
        {
            Api.zoom_to_box(pid, ref a, ref b);
        }

        public void GetCameraPos(ref CameraPos pos)
        {
            Api.get_camera_pos(pid, ref pos);
        }

        public void SetCameraPos(CameraPos pos)
        {
            Api.set_camera_pos(pid, ref pos);
        }

        /// <summary>
        /// may throw DemoModeCallLimitException
        /// </summary>
        public void Invalidate()
        {
            Api.invalidate(pid);
        }

        public float GetDrawTime()
        {
            return Api.get_draw_time(pid);
        }

        public OBJECT_ID[] GetAllObjects()
        {
            var count = Api.get_all_objects(pid, null, 0);
            if (count > 0)
            {
                var res = new OBJECT_ID[count];
                Api.get_all_objects(pid, res, count);
                return res;
            }
            else
                return null;
        }

        /// <summary>
        /// may throw DemoModeCallLimitException
        /// </summary>
        /// <returns></returns>
        public OBJECT_ID[] GetSelected()
        {
            var count = Api.get_selected(pid, null, 0);
            if (count > 0)
            {
                var res = new OBJECT_ID[count];
                Api.get_selected(pid, res, count);
                return res;
            }
            else
                return null;
        }

        /// <summary>
        /// may throw DemoModeCallLimitException
        /// </summary>
        /// <returns></returns>
        public int GetSelectedCount()
        {
            return (int)Api.get_selected(pid, null, 0);
        }

        /// <summary>
        /// may throw DemoModeCallLimitException
        /// </summary>
        /// <returns></returns>
        public OBJECT_ID[] GetVisible()
        {
            var count = Api.get_visible(pid, null, 0);
            if (count > 0)
            {
                var res = new OBJECT_ID[count];
                Api.get_visible(pid, res, count);
                return res;
            }
            else
                return null;
        }

        /// <summary>
        /// may throw DemoModeCallLimitException
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ObjectState[] GetObjectState(OBJECT_ID[] id)
        {
            if (id != null && id.Length > 0)
            {
                var res = new ObjectState[id.Length];
                Api.get_object_state(pid, id, res, (uint)id.Length);
                return res;
            }
            else
                return null;
        }

        public bool IsVisible(OBJECT_ID id)
        {
            return Api.is_visible(pid, id);
        }

        public bool IsTransparent(OBJECT_ID id)
        {
            return Api.is_transparent(pid, id);
        }

        public bool IsSelected(OBJECT_ID id)
        {
            return Api.is_selected(pid, id);
        }

        public bool IsActive(OBJECT_ID id)
        {
            return Api.is_active(pid, id);
        }

        public OBJECT_ID[] GetChildren(OBJECT_ID id)
        {
            return GetChilds(id);
        }

        public OBJECT_ID[] GetChilds(OBJECT_ID id)
        {
            var count = Api.get_childs(pid, id, null, 0);
            if (count > 0)
            {
                var res = new OBJECT_ID[count];
                Api.get_childs(pid, id, res, count);
                return res;
            }
            else
                return null;
        }

        public struct ObjectInfo
        {
            public ObjectType object_type;
            public ElementType element_type;
            public string name;
            public string description;
            public string ifc_entity_name;
            public uint ifc_entity_number;
            public OBJECT_ID parent;
        }

        public ObjectInfo[] GetObjectInfo(OBJECT_ID[] id)
        {
            if (id != null && id.Length > 0)
            {
                ObjectInfo_[] info = new ObjectInfo_[id.Length];
                Api.get_object_info(pid, id, info, (uint)id.Length);

                var res = new ObjectInfo[id.Length];
                for (var i = 0; i < id.Length; i++)
                {
                    res[i].object_type = info[i].object_type;
                    res[i].element_type = info[i].element_type;
                    res[i].name = Marshal.PtrToStringUni(info[i].name);
                    res[i].description = Marshal.PtrToStringUni(info[i].description);
                    res[i].ifc_entity_name = Marshal.PtrToStringAnsi(info[i].ifc_entity_name);
                    res[i].ifc_entity_number = info[i].ifc_entity_number;
                    res[i].parent = info[i].parent;
                }
                return res;
            }
            else
                return null;
        }
        //TODO: function GetObjectInfo* for single ID
        public ObjectInfo GetObjectInfo(OBJECT_ID id)//by GMB
        {
            ObjectInfo[] res = null;
            ObjectInfo_[] info = new ObjectInfo_[1];
            OBJECT_ID[] idt = new OBJECT_ID[] { id };
            Api.get_object_info(pid, idt, info, (uint)1);
            res = new ObjectInfo[1];
            res[0].object_type = info[0].object_type;
            res[0].element_type = info[0].element_type;
            res[0].name = Marshal.PtrToStringUni(info[0].name);
            res[0].description = Marshal.PtrToStringUni(info[0].description);
            res[0].ifc_entity_name = Marshal.PtrToStringAnsi(info[0].ifc_entity_name);
            res[0].ifc_entity_number = info[0].ifc_entity_number;
            res[0].parent = info[0].parent;
            return res[0];
        }
        public void SetUserData(OBJECT_ID id, IntPtr user_data, bool with_all_children)
        {
            Api.set_user_data(pid, id, user_data, with_all_children);
        }

        public IntPtr GetUserData(OBJECT_ID id)
        {
            return Api.get_user_data(pid, id);
        }

        public OBJECT_ID GetProject(OBJECT_ID id)
        {
            return Api.get_project(pid, id);
        }

        public OBJECT_ID GetBuilding(OBJECT_ID id)
        {
            return Api.get_building(pid, id);
        }

        public OBJECT_ID GetStorey(OBJECT_ID id)
        {
            return Api.get_storey(pid, id);
        }

        public OBJECT_ID GetParent(OBJECT_ID id)
        {
            return Api.get_parent(pid, id);
        }

        public struct ObjectInfo2
        {
            public OBJECT_ID project;
            public OBJECT_ID building;
            public OBJECT_ID storey;
            public string tag;
            public IntPtr user_data;
            public string global_id;
        }

        public ObjectInfo2[] GetObjectInfo2(OBJECT_ID[] id)
        {
            if (id != null && id.Length > 0)
            {
                ObjectInfo2_[] info = new ObjectInfo2_[id.Length];
                Api.get_object_info2(pid, id, info, (uint)id.Length);

                var res = new ObjectInfo2[id.Length];
                for (var i = 0; i < id.Length; i++)
                {
                    res[i].project = info[i].project;
                    res[i].building = info[i].building;
                    res[i].storey = info[i].storey;
                    res[i].tag = Marshal.PtrToStringUni(info[i].tag);
                    res[i].user_data = info[i].user_data;
                    res[i].global_id = Marshal.PtrToStringUni(info[i].global_id);
                }
                return res;
            }
            else
                return null;
        }

        public struct Layer
        {
            public LAYER_REF layer_ref;
            public string name;
            public string description;
        }

        public Layer[] GetLayers()
        {
            var count = Api.get_layers(pid, null, 0);
            if (count > 0)
            {
                Layer_[] layers = new Layer_[count];

                count = Math.Min(count, Api.get_layers(pid, layers, (uint)layers.Length));
                var res = new Layer[count];
                for (var i = 0; i < count; i++)
                {
                    res[i].layer_ref = layers[i].layer_ref;
                    res[i].name = Marshal.PtrToStringUni(layers[i].name);
                    res[i].description = Marshal.PtrToStringUni(layers[i].description);
                }
                return res;
            }
            else
                return null;
        }


        public OBJECT_ID[] GetLayerObjects(LAYER_REF id)
        {
            var count = Api.get_layer_objects(pid, id, null, 0);
            if (count > 0)
            {
                var res = new OBJECT_ID[count];
                uint cnt = Api.get_layer_objects(pid, id, res, count);
                return res;
            }
            else
                return null;
        }


        public struct Group
        {
            public GROUP_REF group_ref;
            public string name;
            public string description;
            public string object_type;
            public string global_id;
        }

        public Group[] GetGroups()
        {
            var count = Api.get_groups(pid, null, 0);
            if (count > 0)
            {
                Group_[] groups = new Group_[count];

                count = Math.Min(count, Api.get_groups(pid, groups, (uint)groups.Length));
                var res = new Group[count];
                for (var i = 0; i < count; i++)
                {
                    res[i].group_ref = groups[i].group_ref;
                    res[i].name = Marshal.PtrToStringUni(groups[i].name);
                    res[i].description = Marshal.PtrToStringUni(groups[i].description);
                    res[i].object_type = Marshal.PtrToStringUni(groups[i].object_type);
                    res[i].global_id = Marshal.PtrToStringAnsi(groups[i].global_id);
                }
                return res;
            }
            else
                return null;
        }


        public OBJECT_ID[] GetGroupObjects(GROUP_REF id)
        {
            var count = Api.get_group_objects(pid, id, null, 0);
            if (count > 0)
            {
                var res = new OBJECT_ID[count];
                uint cnt = Api.get_group_objects(pid, id, res, count);
                return res;
            }
            else
                return null;
        }

        public struct Zone
        {
            public ZONE_REF zone_ref;
            public string name;
            public string description;
        }

        public Zone[] GetZones()
        {
            var count = Api.get_zones(pid, null, 0);
            if (count > 0)
            {
                Zone_[] zones = new Zone_[count];

                count = Math.Min(count, Api.get_zones(pid, zones, (uint)zones.Length));
                var res = new Zone[count];
                for (var i = 0; i < count; i++)
                {
                    res[i].zone_ref = zones[i].zone_ref;
                    res[i].name = Marshal.PtrToStringUni(zones[i].name);
                    res[i].description = Marshal.PtrToStringUni(zones[i].description);

                }
                return res;
            }
            else
                return null;
        }

        public struct Zone2
        {
            public ZONE_REF zone_ref;
            public string name;
            public string description;
            public string object_type;
            public string global_id;
        }

        public Zone2[] GetZones2()
        {
            var count = Api.get_zones2(pid, null, 0);
            if (count > 0)
            {
                Zone2_[] zones2 = new Zone2_[count];

                count = Math.Min(count, Api.get_zones2(pid, zones2, (uint)zones2.Length));
                var res = new Zone2[count];
                for (var i = 0; i < count; i++)
                {
                    res[i].zone_ref = zones2[i].zone_ref;
                    res[i].name = Marshal.PtrToStringUni(zones2[i].name);
                    res[i].description = Marshal.PtrToStringUni(zones2[i].description);
                    res[i].object_type = Marshal.PtrToStringUni(zones2[i].object_type);
                    res[i].global_id = Marshal.PtrToStringUni(zones2[i].global_id);
                }
                return res;
            }
            else
                return null;
        }




        public OBJECT_ID[] GetZoneObjects(ZONE_REF id)
        {
            var count = Api.get_zone_objects(pid, id, null, 0);
            if (count > 0)
            {
                var res = new OBJECT_ID[count];
                uint cnt = Api.get_zone_objects(pid, id, res, count);
                return res;
            }
            else
                return null;
        }

        public struct System
        {
            public SYSTEM_REF system_ref;
            public string name;
            public string description;
            public string object_type;
            public string global_id;

        }

        public System[] GetSystems()
        {
            var count = Api.get_systems(pid, null, 0);
            if (count > 0)
            {
                System_[] systems = new System_[count];
                count = Math.Min(count, Api.get_systems(pid, systems, (uint)systems.Length));
                var res = new System[count];
                for (var i = 0; i < count; i++)
                {
                    res[i].system_ref = systems[i].system_ref;
                    res[i].name = Marshal.PtrToStringUni(systems[i].name);
                    res[i].description = Marshal.PtrToStringUni(systems[i].description);
                    res[i].object_type = Marshal.PtrToStringUni(systems[i].object_type);
                    res[i].global_id = Marshal.PtrToStringUni(systems[i].global_id);


                }
                return res;
            }
            else
                return null;
        }

        public OBJECT_ID[] GetSystemObjects(SYSTEM_REF system_ref)
        {
            var count = Api.get_system_objects(pid, system_ref, null, 0);
            if (count > 0)
            {
                var res = new OBJECT_ID[count];
                uint cnt = Api.get_system_objects(pid, system_ref, res, count);
                return res;
            }
            else
                return null;
        }



        public bool DeleteObjectPropertyOrSet(PropertyDef buf)
        {
            return Api.delete_object_property_or_set(pid, ref buf);

        }

        public void AddOrChangeObjectProperty(PropertyDef propertyDef, PropertyData propertyData)
        {

            Api.add_or_change_object_property(pid, ref propertyDef, ref propertyData);

        }


        public struct PropertySet
        {
            public int nr;
            public string name;
        };

        public PropertySet[] GetPropertySets(OBJECT_ID id)
        {
            var count = Api.get_property_sets(pid, id, null, 0);
            if (count > 0)
            {
                PropertySet_[] ps = new PropertySet_[count];
                Api.get_property_sets(pid, id, ps, count);

                var res = new PropertySet[count];
                for (var i = 0; i < res.Length; i++)
                {
                    res[i].nr = ps[i].nr;
                    res[i].name = Marshal.PtrToStringUni(ps[i].name);
                }
                return res;
            }
            else
                return null;
        }

        public struct Property
        {
            public int set_nr;
            public int nr;
            public string name;
            public ValueType value_type;
            public Value value;
        };

        public struct Property2
        {
            public int set_nr;
            public int nr;
            public string name;
            public ValueType value_type;
            public Value value;
            public string unit;
            public IntPtr reserved;
        };


        public struct Value
        {
            public string value_str;
            public double value_num;
        }

        public Property[] GetProperties(OBJECT_ID id, uint propertySetNr)
        {
            PropertyGet pg = new PropertyGet();

            pg.id = id;
            pg.set_nr = propertySetNr;

            var count = Api.get_properties(pid, ref pg, null, 0);
            if (count > 0)
            {
                Property_[] prop = new Property_[count];
                Api.get_properties(pid, ref pg, prop, count);

                var res = new Property[count];
                for (var i = 0; i < res.Length; i++)
                {
                    res[i].set_nr = prop[i].set_nr;
                    res[i].nr = prop[i].nr;
                    res[i].name = Marshal.PtrToStringUni(prop[i].name);
                    res[i].value_type = prop[i].value_type;
                    if (res[i].value_type == ValueType.vt_string)
                        res[i].value.value_str = Marshal.PtrToStringUni(prop[i].value.value_str);
                    else
                        res[i].value.value_num = prop[i].value.value_num;
                }
                return res;
            }
            else
                return null;
        }

        public Property[] FilterProperties(OBJECT_ID id, string setName, string propertyName)
        {
            PropertyFilter pf = new PropertyFilter();

            pf.id = id;
            pf.set_name = setName;
            pf.property_name = propertyName;

            var count = Api.filter_properties(pid, ref pf, null, 0);
            if (count > 0)
            {
                Property_[] prop = new Property_[count];
                Api.filter_properties(pid, ref pf, prop, count);

                var res = new Property[count];
                for (var i = 0; i < res.Length; i++)
                {
                    res[i].set_nr = prop[i].set_nr;
                    res[i].nr = prop[i].nr;
                    res[i].name = Marshal.PtrToStringUni(prop[i].name);
                    res[i].value_type = prop[i].value_type;
                    if (res[i].value_type == ValueType.vt_string)
                        res[i].value.value_str = Marshal.PtrToStringUni(prop[i].value.value_str);
                    else
                        res[i].value.value_num = prop[i].value.value_num;
                }
                return res;
            }
            else
                return null;
        }

        public void SelectProperty(int setNr, int propertyNr, bool selectValue)
        {
            Api.select_property(pid, setNr, propertyNr, selectValue);
        }


        public struct PropertySetData
        {
            public int type;
            public REF property_ref;
            public REF set_ref;
            public string name;
            public string description;
            public int value_type;
            public string value_str;
            public double value_num;
            public string unit;
        }

        //TODO: return empty list instead of null
        //TODO: build tree
        //TODO: use enums
        public PropertySetData[] GetObjectProperties(OBJECT_ID id, int flag)
        {
            var count = Api.get_object_properties(pid, id, flag, null, 0);
            if (count > 0)
            {
                PropertySetData_[] tmp = new PropertySetData_[count];
                Api.get_object_properties(pid, id, flag, tmp, count);

                var res = new PropertySetData[count];
                for (var i = 0; i < res.Length; i++)
                {
                    res[i].type = tmp[i].type;
                    res[i].property_ref = tmp[i].property_ref;
                    res[i].set_ref = tmp[i].set_ref;
                    res[i].name = Marshal.PtrToStringUni(tmp[i].name);
                    res[i].description = Marshal.PtrToStringUni(tmp[i].description);
                    res[i].value_type = tmp[i].value_type;
                    res[i].value_str = Marshal.PtrToStringUni(tmp[i].value_str);
                    res[i].value_num = tmp[i].value_num;
                    res[i].unit = Marshal.PtrToStringAnsi(tmp[i].unit);
                }
                return res;
            }
            else
                return null;
        }

        public struct PropertySetData2
        {
            public int type;
            public REF property_ref;
            public REF set_ref;
            public string name;
            public string description;
            public int value_type;
            public string value_str;
            public double value_num;
            public string unit;
            // from API v5.46
            public int level;           // indentation level
            public string guid;            // only for property sets
            public string ifc_entity_name;
            public byte[] reserved;
        }

        public PropertySetData2[] GetObjectProperties2(OBJECT_ID id, int flag)
        {
            var count = Api.get_object_properties2(pid, id, flag, null, 0);
            if (count > 0)
            {
                PropertySetData2_[] tmp = new PropertySetData2_[count];
                Api.get_object_properties2(pid, id, flag, tmp, count);

                var res = new PropertySetData2[count];
                for (var i = 0; i < res.Length; i++)
                {
                    res[i].type = tmp[i].type;
                    res[i].property_ref = tmp[i].property_ref;
                    res[i].set_ref = tmp[i].set_ref;
                    res[i].name = Marshal.PtrToStringUni(tmp[i].name);
                    res[i].description = Marshal.PtrToStringUni(tmp[i].description);
                    res[i].value_type = tmp[i].value_type;
                    res[i].value_str = Marshal.PtrToStringUni(tmp[i].value_str);
                    res[i].value_num = tmp[i].value_num;
                    res[i].unit = Marshal.PtrToStringAnsi(tmp[i].unit);


                    res[i].level = tmp[i].level;
                    res[i].guid = Marshal.PtrToStringUni(tmp[i].guid); ;
                    res[i].ifc_entity_name = Marshal.PtrToStringAnsi(tmp[i].ifc_entity_name); ;
                    //res[i].reserved = ;
                }
                return res;
            }
            else
                return null;
        }

        public PropertySetData[] GetMaterialProperties(MATERIAL_REF material_ref, int flag)
        {
            var count = Api.get_material_properties(pid, material_ref, flag, null, 0);
            if (count > 0)
            {
                PropertySetData_[] tmp = new PropertySetData_[count];
                Api.get_material_properties(pid, material_ref, flag, tmp, count);

                var res = new PropertySetData[count];
                for (var i = 0; i < res.Length; i++)
                {
                    res[i].type = tmp[i].type;
                    res[i].property_ref = tmp[i].property_ref;
                    res[i].set_ref = tmp[i].set_ref;
                    res[i].name = Marshal.PtrToStringUni(tmp[i].name);
                    res[i].description = Marshal.PtrToStringUni(tmp[i].description);
                    res[i].value_type = tmp[i].value_type;
                    res[i].value_str = Marshal.PtrToStringUni(tmp[i].value_str);
                    res[i].value_num = tmp[i].value_num;
                    res[i].unit = Marshal.PtrToStringAnsi(tmp[i].unit);
                }
                return res;
            }
            else
                return null;
        }

        public PropertySetData[] GetTypeStyleProperties(OBJECT_TYPE_STYLE_REF type_style_ref, int flag)
        {
            var count = Api.get_type_style_properties(pid, type_style_ref, flag, null, 0);
            if (count > 0)
            {
                PropertySetData_[] tmp = new PropertySetData_[count];
                Api.get_type_style_properties(pid, type_style_ref, flag, tmp, count);

                var res = new PropertySetData[count];
                for (var i = 0; i < res.Length; i++)
                {
                    res[i].type = tmp[i].type;
                    res[i].property_ref = tmp[i].property_ref;
                    res[i].set_ref = tmp[i].set_ref;
                    res[i].name = Marshal.PtrToStringUni(tmp[i].name);
                    res[i].description = Marshal.PtrToStringUni(tmp[i].description);
                    res[i].value_type = tmp[i].value_type;
                    res[i].value_str = Marshal.PtrToStringUni(tmp[i].value_str);
                    res[i].value_num = tmp[i].value_num;
                    res[i].unit = Marshal.PtrToStringAnsi(tmp[i].unit);
                }
                return res;
            }
            else
                return null;
        }


        public struct Classification
        {
            public REF cref;
            public string source;
            public string edition;
            public string edition_date;
            public string name;
            public string description;
            public string location;
        };


        public struct ClassificationReference
        {
            public REF cref;
            public REF cbase;
            public string location;
            public string identification;
            public string name;
            public string description;
            public string sort;
        };

        public ClassificationReference[] GetObjectClassificationReferences(OBJECT_ID id)
        {
            var count = Api.get_object_classification_references(pid, id, null, 0);
            if (count > 0)
            {
                var tmp = new ClassificationReference_[count];


                Api.get_object_classification_references(pid, id, tmp, count);

                var res = new ClassificationReference[count];

                for (var i = 0; i < count; ++i)
                    FillReference(out res[i], ref tmp[i]);
                return res;
            }
            else
                return null;
        }

        private void FillReference(out ClassificationReference result, ref ClassificationReference_ source)
        {
            result.cref = source.cref;
            result.cbase = source.cbase;
            result.location = Marshal.PtrToStringUni(source.location);
            result.identification = Marshal.PtrToStringUni(source.identification);
            result.name = Marshal.PtrToStringUni(source.name);
            result.description = Marshal.PtrToStringUni(source.description);
            result.sort = Marshal.PtrToStringUni(source.sort);
        }

        private void FillClassification(out Classification resut, ref Classification_ source)
        {
            resut.cref = source.cref;
            resut.source = Marshal.PtrToStringUni(source.source);
            resut.edition = Marshal.PtrToStringUni(source.edition);
            resut.edition_date = Marshal.PtrToStringUni(source.edition_date);
            resut.name = Marshal.PtrToStringUni(source.name);
            resut.description = Marshal.PtrToStringUni(source.description);
            resut.location = Marshal.PtrToStringUni(source.location);
        }

        public bool GetClassificationData(REF cl, ref Classification classification)
        {
            var api_clasification = new Classification_();

            if (Api.get_classification_data(pid, cl, ref api_clasification))
            {
                FillClassification(out classification, ref api_clasification);
                return true;
            }
            return false;
        }

        public bool GetClassificationReferenceData(REF cl_ref, ref ClassificationReference classification_reference)
        {
            var api_refernece = new ClassificationReference_();

            if (Api.get_classification_reference_data(pid, cl_ref, ref api_refernece))
            {
                FillReference(out classification_reference, ref api_refernece);
                return true;
            }
            return false;
        }

        public Classification[] GetClassifications()
        {
            var count = Api.get_classifications(pid, null, 0);
            if (count > 0)
            {
                var tmp = new Classification_[count];
                Api.get_classifications(pid, tmp, count);
                var res = new Classification[count];
                for (int i = 0; i < count; i++)
                    FillClassification(out res[i], ref tmp[i]);
                return res;
            }
            else
                return null;
        }

        public ClassificationReference[] GetClassificationReferences(string classification_name)
        {
            var count = Api.get_classification_references(pid, classification_name, null, 0);
            if (count > 0)
            {
                var tmp = new ClassificationReference_[count];
                Api.get_classification_references(pid, classification_name, tmp, count);
                var res = new ClassificationReference[count];
                for (int i = 0; i < count; i++)
                    FillReference(out res[i], ref tmp[i]);
                return res;
            }
            else
                return null;
        }

        public double GetUnitFactor()
        {
            return Api.get_unit_factor(pid);
        }

        public bool FirstGeometry(OBJECT_ID id)
        {
            return Api.first_geometry(pid, id);
        }

        public void GetTotalGeometryBounds(ref Bounds bounds)
        {
            Api.get_total_geometry_bounds(pid, ref bounds);
        }

        public Face[] GetGeometry()
        {
            var count = Api.get_geometry(pid, null, 0);
            if (count > 0)
            {
                var res = new Face[count];
                Api.get_geometry(pid, res, count);
                return res;
            }
            else
                return null;
        }

        public Edge[] GetGeometryEdges()
        {
            var count = Api.get_geometry_edges(pid, null, 0);
            if (count > 0)
            {
                var res = new Edge[count];
                Api.get_geometry_edges(pid, res, count);
                return res;
            }
            else
                return null;
        }

        public OBJECT_ID CreateUserObject(OBJECT_ID id, string ifc_entity_name, string name, string description)
        {
            return Api.create_user_object(pid, id, ifc_entity_name, name, description);
        }

        public bool CheckTriangle(Face face)
        {
            return Api.check_triangle(pid, face);
        }

        public bool DeleteUserObject(OBJECT_ID id)
        {
            return Api.delete_user_object(pid, id);
        }

        public Face[] AddGeometryUserObject(OBJECT_ID id, Face[] faces, Color[] cl)
        {

            if (Api.add_geometry_user_object(pid, id, faces, (uint)faces.Length, cl))
            {
                if (faces.Length == 0) return null;
                return faces;
            }
            return null;
        }

        public bool DeleteGeometryUserObject(OBJECT_ID id)
        {
            return Api.delete_geometry_user_object(pid, id);

        }

        public Edge[] SetGeometryEdgesUserObject(OBJECT_ID id, Edge[] edge)
        {

            if (Api.set_geometry_edges_user_object(pid, id, edge, (uint)edge.Length))
            {
                if (edge.Length == 0) return null;
                return edge;
            }

            return null;
        }






        public void GetGeometryColor(ref Color face_color, ref Color line_color)
        {
            Api.get_geometry_color(pid, ref face_color, ref line_color);
        }
        /// <summary>
        /// may throw DemoModeCallLimitException
        /// </summary>
        /// <returns></returns>
        public bool NextGeometry()
        {
            return Api.next_geometry(pid);
        }

        public bool IsOnlineLicence()
        {
            return Api.is_online_licence(pid);
        }


        private static Api.callback_fun onPropertyChangeDelegate;

        public void OnPropertyChange(Api.callback_fun fun)
        {
            onPropertyChangeDelegate = fun;
            Api.on_property_change(pid, Marshal.GetFunctionPointerForDelegate(fun));
        }

        private static Api.callback_fun onMeasureChangeDelegate;

        public Vector3d[] GetObjectCorners(OBJECT_ID id)
        {
            var count = Api.get_object_corners(pid, id, null, 0);
            if (count > 0)
            {
                var res = new Vector3d[count];
                Api.get_object_corners(pid, id, res, count);
                return res;
            }
            else
                return null;
        }

        public Edge[] GetObjectEdges(OBJECT_ID id)
        {
            var count = Api.get_object_edges(pid, id, null, 0);
            if (count > 0)
            {
                var res = new Edge[count];
                Api.get_object_edges(pid, id, res, count);
                return res;
            }
            else
                return null;
        }

        public bool GetObjectArea(OBJECT_ID id, ref MeasuredArea measuredArea)
        {
            return Api.get_object_area(pid, id, ref measuredArea);
        }

        public void OnMeasureChange(Api.callback_fun fun)
        {
            onMeasureChangeDelegate = fun;
            Api.on_measure_change(pid, Marshal.GetFunctionPointerForDelegate(fun));
        }

        public Measure GetMeasure()
        {
            Measure res = new Measure();
            Api.get_measure(pid, ref res);
            return res;
        }

        public MeasureV1 GetMeasureV1()
        {
            MeasureV1 res = new MeasureV1();
            Api.get_measure_v1(pid, ref res);
            return res;
        }
        public OBJECT_ID[] GetMeasureObjects()
        {
            var count = Api.get_measure_objects(pid, null, 0);
            if (count > 0)
            {
                var res = new OBJECT_ID[count];
                Api.get_measure_objects(pid, res, count);
                return res;
            }
            else
                return null;
        }


        public double[] GetMeasureElements()
        {
            var count = Api.get_measure_elements(pid, null, 0);
            if (count > 0)
            {
                var res = new double[count];
                Api.get_measure_elements(pid, res, count);
                return res;
            }
            else
                return null;
        }


        public void ClearMeasure()
        {
            Api.clear_measure(pid);
        }

        public string GetLoadedIfcPath()
        {
            return Marshal.PtrToStringUni(Api.get_loaded_ifc_path(pid));
        }

        public string GetProjectPath(OBJECT_ID project_id)
        {
            return Marshal.PtrToStringUni(Api.get_project_path(pid, project_id));
        }
        public bool ExtractFileFromBvf(OBJECT_ID project_id, [MarshalAs(UnmanagedType.LPWStr)] string to_ifc_file_name)
        {
            return Api.extract_file_from_bvf(pid, project_id, to_ifc_file_name);
        }

        public bool LoadIfc(string path)
        {
            return Api.load_ifc(pid, path);
        }

        public bool LoadIfcFromBuffer(string path, byte[] buf, uint bufSize)
        {
            return Api.load_ifc_from_buffer(pid, path, buf, bufSize);
        }

        public OBJECT_ID CalculateObjectId(string ifcId)
        {
            return Api.calculate_object_id(pid, ifcId);
        }

        private static Api.callback_fun onModelLoadDelegate;

        public void OnModelLoad(Api.callback_fun fun)
        {
            onModelLoadDelegate = fun;
            Api.on_model_load(pid, Marshal.GetFunctionPointerForDelegate(fun));
        }

        public bool OnModelLoadV2(bool isBvfFile)
        {
            return Api.on_model_load_v2(isBvfFile);
        }

        public void OnModelSaveV2()
        {
            Api.on_model_save_v2();
        }

        private static Api.callback_fun onModelClearDelegate;

        public void OnModelClear(Api.callback_fun fun)
        {
            onModelClearDelegate = fun;
            Api.on_model_clear(pid, Marshal.GetFunctionPointerForDelegate(fun));
        }

        private static Api.callback_fun onMainFormCloseDelegate;
        public void OnMainFormClose(Api.callback_fun fun)
        {
            onMainFormCloseDelegate = fun;
            Api.on_main_form_close(pid, Marshal.GetFunctionPointerForDelegate(fun));
        }

        private static Api.callback_fun onSelectionChangeDelegate;

        public void OnSelectionChange(Api.callback_fun fun)
        {
            onSelectionChangeDelegate = fun;
            Api.on_selection_change(pid, Marshal.GetFunctionPointerForDelegate(fun));
        }

        public bool GetClickedPos(ref RelativePos pos)
        {
            return Api.get_clicked_pos(pid, ref pos);
        }

        public bool GetClickedNormal(ref Vector3d normal)
        {
            return Api.get_clicked_normal(pid, ref normal);
        }

        private static Api.callback_fun onDrawDelegate;

        public void OnDraw(Api.callback_fun fun)
        {
            onDrawDelegate = fun;
            Api.on_draw(pid, Marshal.GetFunctionPointerForDelegate(fun));
        }

        public void SetDrawObjectId(uint id)
        {
            Api.set_draw_object_id(pid, id);
        }

        private static Api.callback_fun onDrawObjectClickDelegate;

        public void OnDrawObjectClick(Api.callback_fun fun)
        {
            onDrawObjectClickDelegate = fun;
            Api.on_draw_object_click(pid, Marshal.GetFunctionPointerForDelegate(fun));
        }

        //
        private static Api.callback_fun onUndoRedoAction;

        public void OnUndoRedoAction(Api.callback_fun fun)
        {
            onUndoRedoAction = fun;
            Api.on_undo_redo_action(pid, Marshal.GetFunctionPointerForDelegate(fun));
        }
        //

        public uint GetDrawObjectId()
        {
            return Api.get_draw_object_id(pid);
        }

        public void DrawPoint(Point point)
        {
            Api.draw_point(pid, ref point);
        }

        public void DrawLine(Line line)
        {
            Api.draw_line(pid, ref line);
        }

        public void DrawAbsoluteLine(AbsoluteLine line)
        {
            Api.draw_absolute_line(pid, ref line);
        }

        public enum DrawImageError
        {
            de_no_error,                 // successful execution
            de_general_error,            // general error
            de_file_open_error,          // file missing or unauthorized access etc.
            de_unsupported_format,       // unsupported graphics format (supported formats: BMP, JPEG, GIF, PNG, TIFF, ICO)
            de_missing_buffer,           // missing buffer
            de_wrong_height,             // wrong height value (must be greater then 0)
            de_missing_product,          // product missing
        };

        // use in on_draw() event
        // returns one of DrawImageError enum values 
        public DrawImageError DrawImage(Image image)
        {
            return (DrawImageError)Api.draw_image(pid, ref image);
        }

        // use in on_draw() event
        // returns one of DrawImageError enum values 
        public DrawImageError DrawAbsoluteImage(AbsoluteImage absoluteImage)
        {
            return (DrawImageError)Api.draw_absolute_image(pid, ref absoluteImage);
        }

        // use in on_draw() event
        // returns one of DrawImageError enum values 
        public DrawImageError DrawImageFile(ImageFile imageFile)
        {
            return (DrawImageError)Api.draw_image_file(pid, ref imageFile);
        }

        // use in on_draw() event
        // returns one of DrawImageError enum values 
        public DrawImageError DrawAbsoluteImageFile(AbsoluteImageFile absoluteImageFile)
        {
            return (DrawImageError)Api.draw_absolute_image_file(pid, ref absoluteImageFile);
        }

        public void DrawSphere(Sphere sphere)
        {
            Api.draw_sphere(pid, ref sphere);
        }

        public void LoadIconImage(string imagePath)
        {
            Api.load_icon_image(pid, imagePath);
        }

        public void DrawIcon(Icon icon)
        {
            Api.draw_icon(pid, ref icon);
        }

        public void DrawLabel(Label label)
        {
            Api.draw_label(pid, ref label);
        }

        public void GetElementPos(ref RelativePos pos)
        {
            Api.get_element_pos(pid, ref pos);
        }

        public bool SaveFileAs(string file_name, SaveType save_type)
        {
            return Api.save_file_as(pid, file_name, (int)save_type);
        }

        public string GetLanguage()
        {
            return Marshal.PtrToStringUni(Api.get_language(pid));
        }

        public bool LoadTexts(string filePath)
        {
            return Api.load_texts(pid, filePath);
        }

        public string GetText(string txtId)
        {
            return Marshal.PtrToStringUni(Api.get_text(pid, txtId));
        }

        public string GetTextGlobal(string txtId)
        {
            return Marshal.PtrToStringUni(Api.get_text_global(pid, txtId));
        }

        public string GetEntityTypeName(string ifcEntityName)
        {
            return Marshal.PtrToStringUni(Api.get_entity_type_name(pid, ifcEntityName));
        }

        public OBJECT_ID GetObjectBelowMouse(int x, int y)
        {
            return Api.get_object_below_mouse(pid, x, y);
        }

        public int RegisterUndoAction([MarshalAs(UnmanagedType.LPWStr)] string action_name)
        {
            return Api.register_undo_action(pid, action_name);
        }

        public int CreateTab(string name)
        {
            return Api.create_tab(pid, name);
        }

        public int CreateGroup(int tabId, string name)
        {
            return Api.create_group(pid, tabId, name);
        }

        public void ShowGroup(int groupId, bool visible)
        {
            Api.show_group(pid, groupId, visible);
        }

        private static List<Api.callback_fun> guiDelegates = new List<Api.callback_fun>();

        public int CreateButton(int groupId, Api.callback_fun fun)
        {
            guiDelegates.Add(fun);
            return Api.create_button(pid, groupId, Marshal.GetFunctionPointerForDelegate(fun));
        }

        public int CreateSmallButton(int groupId, Api.callback_fun fun)
        {
            guiDelegates.Add(fun);
            return Api.create_small_button(pid, groupId, Marshal.GetFunctionPointerForDelegate(fun));
        }

        public int CreateCheckbox(int groupId, Api.callback_fun fun)
        {
            guiDelegates.Add(fun);
            return Api.create_checkbox(pid, groupId, Marshal.GetFunctionPointerForDelegate(fun));
        }

        public int CreateRadioButton(int groupId, Api.callback_fun fun)
        {
            guiDelegates.Add(fun);
            return Api.create_radio_button(pid, groupId, Marshal.GetFunctionPointerForDelegate(fun));
        }

        public int CreateDropdownButton(int groupId, Api.callback_fun fun)
        {
            guiDelegates.Add(fun);
            return Api.create_dropdown_button(pid, groupId, Marshal.GetFunctionPointerForDelegate(fun));
        }

        public int CreateSubButton(int groupId, bool small)
        {
            return Api.create_sub_button(pid, groupId, small);
        }

        public int CreateSubButton2(int groupId, bool small, Api.callback_fun fun)
        {
            guiDelegates.Add(fun);
            return Api.create_sub_button2(pid, groupId, small, Marshal.GetFunctionPointerForDelegate(fun));
        }

        public int CreateSeparator(string caption)
        {
            return Api.create_separator(pid, caption);
        }

        public int AddButton(int parentButtonId, int buttonId)
        {
            return Api.add_button(pid, parentButtonId, buttonId);
        }

        public void AddSeparator(int parentButtonId, string caption)
        {
            Api.add_separator(pid, parentButtonId, caption);
        }

        public void SetButtonText(int buttonId, string caption, string hint)
        {
            Api.set_button_text(pid, buttonId, caption, hint);
        }

        public void SetButtonImage(int buttonId, string largeImgPath)
        {
            Api.set_button_image(pid, buttonId, largeImgPath);
        }

        public void SetButtonSmallImage(int buttonId, string smallImgPath)
        {
            Api.set_button_small_image(pid, buttonId, smallImgPath);
        }

        public void SetButtonShortcut(int buttonId, string shrtcut)
        {
            Api.set_button_shortcut(pid, buttonId, shrtcut);
        }

        public void SetButtonState(int buttonId, bool enabled, bool down)
        {
            Api.set_button_state(pid, buttonId, enabled, down);
        }

        public void SetButtonDown(int buttonId, bool down)
        {
            Api.set_button_down(pid, buttonId, down);
        }

        public void SetButtonGUID(int buttonId, string str)
        {
            Api.set_button_guid(pid, buttonId, str);
        }

        public void EnableButton(int buttonId, bool enable)
        {
            Api.enable_button(pid, buttonId, enable);
        }

        public void ShowButton(int buttonId, bool show)
        {
            Api.show_button(pid, buttonId, show);
        }

        public int ClickedButton()
        {
            return Api.clicked_button(pid);
        }

        public void BeginControlGroup()
        {
            Api.begin_control_group(pid);
        }

        private static Api.callback_fun onContextMenuDelegate;

        public void OnContextMenu(Api.callback_fun fun)
        {
            onContextMenuDelegate = fun;
            Api.on_context_menu(pid, Marshal.GetFunctionPointerForDelegate(fun));
        }

        public void AddContextButton(int buttonId)
        {
            Api.add_context_button(pid, buttonId);
        }

        public void AddContextButtonWithSeparator(int buttonId)
        {
            Api.add_context_button_with_separator(pid, buttonId);
        }

        public void ClearContextMenu()
        {
            Api.clear_context_menu(pid);
        }

        public int CreateGalleryButton(int groupId, bool small, Api.callback_fun fun)
        {
            guiDelegates.Add(fun);
            return Api.create_gallery_button(pid, groupId, small, fun);
        }

        public void SetGalleryStyle(int galleryId, int minColCount, int style)
        {
            Api.set_gallery_style(pid, galleryId, minColCount, style);
        }

        public int CreateGalleryCategory(int galleryId)
        {
            return Api.create_gallery_category(pid, galleryId);
        }

        public void SetGalleryCategoryText(int galleryCategoryId, string caption)
        {
            Api.set_gallery_category_text(pid, galleryCategoryId, caption);
        }

        public void SetGalleryCategoryStyle(int galleryCategoryId, int displayTexts, int textsPosistion)
        {
            Api.set_gallery_category_style(pid, galleryCategoryId, displayTexts, textsPosistion);
        }

        public void DeleteGalleryCategory(int galleryCategoryId)
        {
            Api.delete_gallery_category(pid, galleryCategoryId);
        }

        public int CreateGalleryItem(int galleryCategoryId, Api.callback_fun fun)
        {
            guiDelegates.Add(fun);
            return Api.create_gallery_item(pid, galleryCategoryId, fun);
        }

        public void SetGalleryItemText(int galleryItemId, string caption, string descrption)
        {
            Api.set_gallery_item_text(pid, galleryItemId, caption, descrption);
        }

        public void SetGalleryItemImage(int galleryItemId, string path)
        {
            Api.set_gallery_item_image(pid, galleryItemId, path);
        }

        public void DeleteGalleryItem(int galleryItemId)
        {
            Api.delete_gallery_item(pid, galleryItemId);
        }

        public void ClearGallery(int galleryId)
        {
            Api.clear_gallery(pid, galleryId);
        }

        private static Api.callback_fun onGalleryItemContextMenu;

        public void OnGalleryItemContextMenu(Api.callback_fun fun)
        {
            onGalleryItemContextMenu = fun;
            Api.on_gallery_item_context_menu(pid, fun);
        }

        public bool IsCenterView()
        {
            return Api.is_center_view(pid);
        }

        public int GetActiveTab()
        {
            return Api.get_active_tab(pid);
        }

        private static Api.callback_fun onTabChangeDelegate;

        public void OnTabChange(Api.callback_fun fun)
        {
            onTabChangeDelegate = fun;
            Api.on_tab_change(pid, Marshal.GetFunctionPointerForDelegate(fun));
        }

        public ColorRGB GetGuiColor(GuiColorId cl)
        {
            return Api.get_gui_color(pid, (int)cl);
        }

        public Rect GetViewRect()
        {
            Rect rect = new Rect();
            Api.get_view_rect(pid, ref rect);
            return rect;
        }

        public void ShowIfcStructureGrid(bool forceShowProperties)
        {
            Api.show_ifc_structure_grid(pid, forceShowProperties);
        }

        public void SetRibbonState(int state)
        {
            Api.set_ribbon_state(pid, state);
        }

        public void SetGridsState(int state)
        {
            Api.set_grids_state(pid, state);
        }
        public bool GetGridsState()
        {
            return Api.get_grids_state(pid);
        }

        public bool IsTouchMode()
        {
            return Api.is_touch_mode(pid);
        }

        public byte[] GetViewState(int flags = 0)
        {
            var count = Api.get_view_state(pid, flags, null, 0);

            if (count > 0)
            {
                var res = new byte[count];
                Api.get_view_state(pid, flags, res, count);
                return res;
            }
            else
                return null;
        }

        public void SetViewState(byte[] buf, int flags = 0)
        {
            Api.set_view_state(pid, flags, buf, GetLenth(buf));
        }

        /// <summary>
        /// may throw DemoModeCallLimitException
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="buf"></param>
        /// <returns></returns>
        public byte[] RenderScreenshot(uint w, uint h)
        {
            if (w > 0 && h > 0)
            {
                var res = new byte[w * h * 3];

                if (Api.render_screenshot(pid, w, h, res))
                    return res;
            }
            return null;
        }

        public void SetScreenshotStyle(int style_or_flags)
        {
            Api.set_screenshot_style(pid, style_or_flags);
        }

        public Bounds[] Get2DBounds(OBJECT_ID[] id, int flags)
        {
            if (id != null && id.Length > 0)
            {
                var res = new Bounds[id.Length];
                Api.get_2d_bounds(pid, id, res, (uint)id.Length, flags);
                return res;
            }
            else
                return null;
        }


        public Vector3d[] Get2DBounds(RelativePos[] pos, int flags)
        {
            if (pos != null && pos.Length > 0)
            {
                var res = new Vector3d[pos.Length];
                Api.project_to_2d(pid, pos, res, (uint)pos.Length, flags);
                return res;
            }
            else
                return null;
        }

        public Vector3d[] ProjectTo2D(RelativePos[] pos, int flags)
        {
            if (pos != null && pos.Length > 0)
            {
                var res = new Vector3d[pos.Length];
                Api.project_to_2d(pid, pos, res, (uint)pos.Length, flags);
                return res;
            }
            else
                return null;
        }

        public void RecalcCrossSections(int flags = 0)
        {
            Api.recalc_cross_sections(pid, flags);
        }

        public void SetCrossSectionsStyle(CrossSetionColor style, ColorRGB color, bool bold)
        {
            Api.set_cross_sections_style(pid, (int)style, color, bold);
        }

        //---deprecated---
        public void SetColor(OBJECT_ID id, ColorRGB cl, bool transparent)
        {
            Api.set_color(pid, id, cl, transparent);
        }

        public void SetColorObject(OBJECT_ID id, ColorRGB cl, bool transparent)
        {
            Api.set_color_object(pid, id, cl, transparent);
        }

        public void SetDefaultColor(OBJECT_ID id)
        {
            Api.set_default_color(pid, id);
        }

        public void SetVisible(OBJECT_ID id, VisibleType vt)
        {
            Api.set_visible(pid, id, (int)vt);
        }

        public void SetVisibleObject(OBJECT_ID id, VisibleType vt)
        {
            Api.set_visible_object(pid, id, (int)vt);
        }

        public void SetVisibleMany(OBJECT_ID[] id, VisibleType vt)
        {
            Api.set_visible_many(pid, id, GetLenth(id), (int)vt);
        }

        public void SetVisibleManyObjects(OBJECT_ID[] id, VisibleType vt)
        {
            Api.set_visible_many_objects(pid, id, GetLenth(id), (int)vt);
        }

        public double GetVolume(OBJECT_ID id)
        {
            return Api.get_volume(pid, id);
        }

        public Vector3d GetCentroid(OBJECT_ID id)
        {
            double weight;
            Vector3d centroid;
            Api.get_centroid(pid, id, out centroid, out weight);

            return centroid;
        }

        public Tuple<Vector3d, double> GetCentroidWeight(OBJECT_ID id)
        {
            double weight;
            Vector3d centroid;

            Api.get_centroid(pid, id, out centroid, out weight);

            return new Tuple<Vector3d, double>(centroid, weight);
        }

        public DirectionCamera GetDirectionCamera()
        {
            DirectionCamera res = new DirectionCamera();
            Api.get_direction_camera(pid, ref res);
            return res;
        }

        public void SetDirectionCamera(ref DirectionCamera cam)
        {
            Api.set_direction_camera(pid, ref cam);
        }

        public enum MeasureTypeFlags : int
        {
            change_tab = 0,
            dont_change_tab = 0x0001,
        }

        public void SetMeasureType(int type, MeasureTypeFlags flags)
        {
            Api.set_measure_type(pid, type, (int)flags);
        }

        public double GetTotalArea(OBJECT_ID id)
        {
            return Api.get_total_area(pid, id);
        }

        public bool BeginReadBvf(string name)
        {
            return Api.begin_read_bvf(pid, name);
        }

        public bool BeginReadBvfV2(string name)
        {
            return Api.begin_read_bvf_v2(pid, name);
        }

        public uint ReadBvf(byte[] buf, uint size)
        {
            return Api.read_bvf(pid, buf, size);
        }

        public uint ReadBvfV2(byte[] buf, uint size)
        {
            return Api.read_bvf_v2(pid, buf, size);
        }

        public bool SaveBvfSubFileV2(string name)
        {
            return Api.save_bvf_sub_file_v2(pid, name);
        }

        public string[] GetBvfDirFileListV2()
        {
            return Api.get_bvf_dir_file_list_v2(pid);
        }

        public uint GetBvfSize()
        {
            return Api.get_bvf_size(pid);
        }

        public uint GetBvfSizeV2()
        {
            return Api.get_bvf_size_v2(pid);
        }

        public byte[] ReadBvf()
        {
            var count = Api.get_bvf_size(pid);
            if (count > 0)
            {
                var res = new byte[count];
                Api.read_bvf(pid, res, count);
                return res;
            }
            else
                return null;
        }


        private static Api.callback_fun onSaveBvfDelegate;
        public void OnSaveBvf(Api.callback_fun fun)
        {
            onSaveBvfDelegate = fun;
            Api.on_save_bvf(pid, Marshal.GetFunctionPointerForDelegate(fun));
        }

        public bool BeginWriteBvf(string name)
        {
            return Api.begin_write_bvf(pid, name);
        }

        public bool BeginWriteBvfV2(string name)
        {
            return Api.begin_write_bvf_v2(pid, name);
        }

        public bool WriteBvf(byte[] buf)
        {
            return Api.write_bvf(pid, buf, GetLenth(buf));
        }

        public bool WriteBvfV2(byte[] buf)
        {
            return Api.write_bvf_v2(pid, buf, GetLenth(buf));
        }

        public bool WriteFileToBvfV2(string externalFileName)
        {
            return Api.write_file_to_bvf_v2(pid, externalFileName);
        }

        private static Api.bool_callback_fun canClearModelDelegate;
        public void CanClearModel(Api.bool_callback_fun fun)
        {
            canClearModelDelegate = fun;
            Api.can_clear_model(pid, Marshal.GetFunctionPointerForDelegate(fun));
        }

        public bool FileWasAdded()
        {
            return Api.file_was_added(pid);
        }

        public void NeedSaveBvf(bool flag)
        {
            Api.need_save_bvf(pid, flag);
        }

        public bool SaveFile()
        {
            return Api.save_file(pid);
        }

        public void NeedSaveChanges(bool flag)
        {
            Api.need_save_changes(pid, flag);
        }

        private static Api.callback_fun onHoverDelegate;
        public void OnHover(Api.callback_fun fun)
        {
            onHoverDelegate = fun;
            Api.on_hover(pid, Marshal.GetFunctionPointerForDelegate(fun));
        }

        public void SetHoverObjects(OBJECT_ID[] id)
        {
            Api.set_hover_objects(pid, id, GetLenth(id));
        }

        public RelativePos GetHoveredPos()
        {
            RelativePos pos = new RelativePos();
            Api.get_hovered_pos(pid, ref pos);
            return pos;
        }
        public RelativePos GetDroppedPos()
        {
            RelativePos pos = new RelativePos();
            Api.get_dropped_pos(pid, ref pos);
            return pos;
        }


        public CuttingPlane[] GetCuttingPlanes()
        {
            var count = Api.get_cutting_planes(pid, null, 0);
            if (count > 0)
            {
                var res = new CuttingPlane[count];
                Api.get_cutting_planes(pid, res, count);
                return res;
            }
            else
                return null;
        }

        public void SetCuttingPlanes(CuttingPlane[] buf)
        {
            Api.set_cutting_planes(pid, buf, GetLenth(buf));
        }

        public OBJECT_ID[] GetActive()
        {
            var count = Api.get_active(pid, null, 0);
            if (count > 0)
            {
                var res = new OBJECT_ID[count];
                Api.get_active(pid, res, count);
                return res;
            }
            else
                return null;
        }

        public OBJECT_ID GetCurrentProduct()
        {
            return Api.get_current_product(pid);
        }

        public bool HasRepresentation(OBJECT_ID id)
        {
            return Api.has_representation(pid, id);
        }

        public bool GetBounds(OBJECT_ID id, ref Bounds bounds)
        {
            return Api.get_bounds(pid, id, ref bounds);
        }

        public Vector3d GetOffset(OBJECT_ID id)
        {
            Vector3d offset = new Vector3d();
            Api.get_offset(pid, id, ref offset);
            return offset;
        }

        public void SetOffset(OBJECT_ID id, Vector3d offset)
        {
            Api.set_offset(pid, id, ref offset);
        }

        public Material[] GetObjectMaterials(OBJECT_ID id)
        {
            var count = Api.get_object_materials(pid, id, null, 0);
            if (count > 0)
            {
                Material_[] ps = new Material_[count];
                Api.get_object_materials(pid, id, ps, count);

                var res = new Material[count];
                for (var i = 0; i < res.Length; i++)
                {
                    res[i].material_ref = ps[i].material_ref;
                    res[i].name = Marshal.PtrToStringUni(ps[i].name);
                    res[i].description = Marshal.PtrToStringUni(ps[i].description);
                }
                return res;
            }
            else
                return null;
        }

        public Material2[] GetObjectMaterials2(OBJECT_ID id)
        {
            var count = Api.get_object_materials2(pid, id, null, 0);
            if (count > 0)
            {
                Material2_[] ps = new Material2_[count];
                Api.get_object_materials2(pid, id, ps, count);

                var res = new Material2[count];
                for (var i = 0; i < res.Length; i++)
                {
                    res[i].material_ref = ps[i].material_ref;
                    res[i].name = Marshal.PtrToStringUni(ps[i].name);
                    res[i].description = Marshal.PtrToStringUni(ps[i].description);
                    res[i].ifc_entity_name = Marshal.PtrToStringAnsi(ps[i].ifc_entity_name);
                }
                return res;
            }
            else
                return null;
        }
        public Material3[] GetObjectMaterials3(OBJECT_ID id)
        {
            var count = Api.get_object_materials3(pid, id, null, 0);
            if (count > 0)
            {
                Material3_[] ps = new Material3_[count];
                Api.get_object_materials3(pid, id, ps, count);

                var res = new Material3[count];
                for (var i = 0; i < res.Length; i++)
                {
                    res[i].material_ref = ps[i].material_ref;
                    res[i].name = Marshal.PtrToStringUni(ps[i].name);
                    res[i].description = Marshal.PtrToStringUni(ps[i].description);
                    res[i].ifc_entity_name = Marshal.PtrToStringAnsi(ps[i].ifc_entity_name);
                    res[i].layer_thickness = ps[i].layer_thickness;
                    res[i].is_ventilated = ps[i].is_ventilated;
                }
                return res;
            }
            else
                return null;
        }

        public ObjectTypeStyle get_object_type_style(OBJECT_ID id)
        {
            ObjectTypeStyle res = null;
            var tmp = new ObjectTypeStyle_();

            if (Api.get_object_type_style(pid, id, ref tmp))
            {
                res = new ObjectTypeStyle();
                res.typestyle_ref = tmp.typestyle_ref;
                res.name = Marshal.PtrToStringUni(tmp.name);
                res.description = Marshal.PtrToStringUni(tmp.description);
                res.ifc_entity_name = Marshal.PtrToStringAnsi(tmp.ifc_entity_name);
            }
            return res;
        }

        public ObjectTypeStyle2 GetObjectTypeStyle2(OBJECT_ID id)
        {
            ObjectTypeStyle2 res = null;
            var tmp = new ObjectTypeStyle2_();

            if (Api.get_object_type_style2(pid, id, ref tmp))
            {
                res = new ObjectTypeStyle2();
                res.typestyle_ref = tmp.typestyle_ref;
                res.name = Marshal.PtrToStringUni(tmp.name);
                res.description = Marshal.PtrToStringUni(tmp.description);
                res.ifc_entity_name = Marshal.PtrToStringAnsi(tmp.ifc_entity_name);
                res.global_id = Marshal.PtrToStringUni(tmp.global_id);
            }
            return res;
        }

        public void DrawPyramid(ref Pyramid pyramid)
        {
            Api.draw_pyramid(pid, ref pyramid);
        }

        public SOLID_ID CreateIntersectionSolid(OBJECT_ID object_a, OBJECT_ID object_b)
        {

            return Api.create_intersection_solid(pid, object_a, object_b);
        }

        public void DeleteSolid(SOLID_ID id)
        {
            Api.delete_solid(pid, id);
        }

        public void DrawSolid(SOLID_ID solid_id, Color color)
        {
            Api.draw_solid(pid, solid_id, color);
        }

        public double GetSolidVolume(SOLID_ID id)
        {
            return Api.get_solid_volume(pid, id);

        }

        public double GetSolidArea(SOLID_ID id)
        {
            return Api.get_solid_area(pid, id);
        }

        public Vertex GetSolidCenterPoint(SOLID_ID id)
        {
            Vertex center = new Vertex();
            Api.get_solid_center_point(pid, id, ref center);
            return center;
        }

        public void ZoomToSolids(SOLID_ID[] id, uint count)
        {
            Api.zoom_to_solids(pid, id, count);
        }



        public void ZoomToObjects(OBJECT_ID[] id, uint count)
        {
            Api.zoom_to_objects(pid, id, count);
        }

        public int GetGuiTheme()
        {
            return Api.get_gui_theme(pid);
        }

        public int MessageBox(string title, string message, int flags)
        {
            return Api.message_box(pid, title, message, flags);
        }

        public void SetOpenDialogParams(string filter, string directory)
        {
            Api.set_open_dialog_params(pid, filter, directory);
        }

        public bool OpenFileDialog(bool multiple_files)
        {
            return Api.open_file_dialog(pid, multiple_files);
        }

        public uint GetOpenDialogFileCount()
        {
            return Api.get_open_dialog_file_count(pid);
        }

        public string GetOpenDialogFileNr(uint nr)
        {
            return Marshal.PtrToStringUni(Api.get_open_dialog_file_nr(pid, nr));
        }

        public void SetSaveDialogParams(string filter, string directory, string file_name)
        {
            Api.set_save_dialog_params(pid, filter, directory, file_name);
        }

        public void SetSaveDialogDefaultExtension(string extension)
        {
            Api.set_save_dialog_default_extension(pid, extension);
        }

        public bool SaveDialog(bool select_directory_only)
        {
            return Api.save_dialog(pid, select_directory_only);
        }

        public string GetSaveDialogName()
        {
            return Marshal.PtrToStringUni(Api.get_save_dialog_name(pid));
        }

        public enum ProgressBarStyle
        {
            OneBar = 0,
            TwoBars = 1
        }

        public void ShowProgressBar(bool show, ProgressBarStyle style, string title)
        {
            Api.show_progress_bar(pid, show, (int)style, title);
        }

        public bool UpdateProgressBar(int percent, string message, int second_percent, string second_message)
        {
            return Api.update_progress_bar(pid, percent, message, second_percent, second_message);
        }


        public Property2[] GetProperties2(OBJECT_ID id, uint propertySetNr)
        {
            PropertyGet pg = new PropertyGet();

            pg.id = id;
            pg.set_nr = propertySetNr;

            var count = Api.get_properties2(pid, ref pg, null, 0);
            if (count > 0)
            {
                Property2_[] prop = new Property2_[count];
                Api.get_properties2(pid, ref pg, prop, count);

                var res = new Property2[count];
                for (var i = 0; i < res.Length; i++)
                {
                    res[i].set_nr = prop[i].set_nr;
                    res[i].nr = prop[i].nr;
                    res[i].name = Marshal.PtrToStringUni(prop[i].name);
                    res[i].value_type = prop[i].value_type;
                    res[i].unit = Marshal.PtrToStringAnsi(prop[i].unit);
                    res[i].reserved = prop[i].reserved;
                    if (res[i].value_type == ValueType.vt_string)
                        res[i].value.value_str = Marshal.PtrToStringUni(prop[i].value.value_str);
                    else
                        res[i].value.value_num = prop[i].value.value_num;
                }
                return res;
            }
            else
                return null;
        }

        private static Api.callback_tab_sheet_placement onTabSheetPlacementDelegate;
        public void OnTabSheetPlacement(Api.callback_tab_sheet_placement fun)
        {
            onTabSheetPlacementDelegate = fun;
            Api.on_tab_sheet_placement(pid, Marshal.GetFunctionPointerForDelegate(fun));
        }

        private static Api.callback_tab_sheet_change OnTabSheetChangeDelegate;
        public void on_tab_sheet_change(Api.callback_tab_sheet_change fun)
        {
            OnTabSheetChangeDelegate = fun;
            Api.on_tab_sheet_change(pid, Marshal.GetFunctionPointerForDelegate(fun));
        }

        public byte[] GetMeasureState()
        {
            var count = Api.get_measure_state(pid, null, 0);
            if (count > 0)
            {
                var res = new byte[count];
                Api.get_measure_state(pid, res, count);
                return res;
            }
            else
                return null;
        }

        public void SetMeasureState(int flags, byte[] buf)
        {
            Api.set_measure_state(pid, flags, buf, GetLenth(buf));
        }


        public uint GetMeasureCount()
        {
            return Api.get_measure_count(pid);
        }


        public byte[] GetDetailedMeasureState(uint measure_nr, ref MeasureDetail details)
        {
            var count = Api.get_detailed_measure_state(pid, measure_nr, null, 0, ref details);
            if (count > 0)
            {
                var res = new byte[count];
                Api.get_detailed_measure_state(pid, measure_nr, res, (uint)res.Length, ref details);
                return res;
            }
            else
                return null;
        }

        enum DetailedMeasureFlag
        {
            dm_clear = 1,
            dm_refresh = 2,
        }

        public void SetDetailedMeasureState(int flags, byte[] state)
        {
            Api.set_detailed_measure_state(pid, flags, state, GetLenth(state));
        }

        public void ChangeMeasureStateDensity(byte[] state, double density)
        {
            Api.change_measure_state_density(pid, state, density);
        }

        public struct ConstructMeasureState
        {
            public MeasureType type;

            public OBJECT_ID id;
            public Plane plane;
            public Segment segment;
        }


        public ConstructMeasureState[] ConstructMeasureStateFun(byte[] buf)
        {
            var count = Api.construct_measure_state(pid, null, buf, GetLenth(buf));
            if (count > 0)
            {
                var res = new ConstructMeasureState[count];


                var tab = new ConstructMeasureState_[count];
                Api.construct_measure_state(pid, tab, buf, GetLenth(buf));
                for (int i = 0; i < count; i++)
                {
                    res[i].id.id = tab[i].id.id;
                    res[i].plane.d = tab[i].plane.d;
                    res[i].plane.normal.x = tab[i].plane.normal.x;
                    res[i].plane.normal.y = tab[i].plane.normal.y;
                    res[i].plane.normal.z = tab[i].plane.normal.z;
                    res[i].segment.first.x = tab[i].segment.first.x;
                    res[i].segment.first.y = tab[i].segment.first.y;
                    res[i].segment.first.z = tab[i].segment.first.z;
                    res[i].segment.second.x = tab[i].segment.second.x;
                    res[i].segment.second.y = tab[i].segment.second.y;
                    res[i].segment.second.z = tab[i].segment.second.z;
                    res[i].type = tab[i].type;

                }
                return res;
            }
            else
                return null;
        }


        private static Api.callback_fun onObjectListChangedDelegate;

        public void OnObjectListChanged(Api.callback_fun fun)
        {
            onObjectListChangedDelegate = fun;
            Api.on_object_list_changed(pid, Marshal.GetFunctionPointerForDelegate(fun));
        }



        public bool AddIfc(string path)
        {
            return Api.add_ifc(pid, path);
        }

        public uint GetLoadedIfcFilesCount()
        {
            return Api.get_loaded_ifc_files_count(pid);
        }

        public string GetLoadedIfcFilename(uint file_index)
        {
            return Marshal.PtrToStringUni(Api.get_loaded_ifc_filename(pid, file_index));
        }

        public PluginStatus GetPluginStatus(string dll_name)
        {
            PluginStatus status = new PluginStatus();
            Api.get_plugin_status(pid, dll_name, ref status);
            return status;
        }

        public PluginStatus2 GetPluginStatus2(string dll_name)
        {
            PluginStatus2 status = new PluginStatus2();
            Api.get_plugin_status2(pid, dll_name, ref status);
            return status;
        }

        public bool SendPluginMassage(string dll_name, ref PluginMessage message)
        {
            return Api.send_plugin_massage(pid, dll_name, ref message);
        }

        private static Api.callback_plugin_message onPluginMessageDelegate;
        public void OnPluginMessage(Api.callback_plugin_message fun)
        {
            onPluginMessageDelegate = fun;
            Api.on_plugin_message(pid, Marshal.GetFunctionPointerForDelegate(fun));
        }

        public int CreateTabSheet(string name, bool allow_close)
        {
            return Api.create_tab_sheet(pid, name, allow_close);
        }

        public void ActivateTabSheet(int buttonId, int id)
        {
            Api.activate_tab_sheet(pid, id);
        }

        public void CloseTabSheet(int buttonId, int id)
        {
            Api.close_tab_sheet(pid, id);
        }

        public void ShowTabSheet(int id, bool show)
        {
            Api.show_tab_sheet(pid, id, show);
        }

        public bool IsActiveTabSheet(int id)
        {
            return Api.is_active_tab_sheet(pid, id);
        }

        public bool ItemCommand(string item_name, string command)
        {
            return Api.item_command(pid, item_name, command);
        }

        public void SetItemValue(string item_name, double value)
        {
            Api.set_item_value(pid, item_name, value);
        }

        public double GetItemValue(string item_name, string value_type)
        {
            return Api.get_item_value(pid, item_name, value_type);
        }

        /*public void OnPropertyChange(Api.callback_fun fun)
        {
            onPropertyChangeDelegate = fun;
            Api.on_property_change(pid, Marshal.GetFunctionPointerForDelegate(fun));
        }
        guiDelegates.Add(fun);
            return Api.create_button(pid, groupId, Marshal.GetFunctionPointerForDelegate(fun));*/

        public double RegisterOnButtonClick(string item_name, Api.callback_fun fun)
        {
            guiDelegates.Add(fun);
            return Api.register_on_button_click(pid, item_name, Marshal.GetFunctionPointerForDelegate(fun));
        }

        public ProjectOffset[] GetProjectsOffsets()
        {
            var count = Api.get_projects_offsets(pid, null, 0);
            if (count > 0)
            {
                var res = new ProjectOffset[count];
                Api.get_projects_offsets(pid, res, count);
                return res;
            }
            else
                return null;
        }
        //---------

        public void BvZipAdd(string zipFileName, string addFileName)
        {
            Api.bv_zip_add(pid, zipFileName, addFileName);
        }

        public bool BvZipGet(string zipFileName, string addFileName, string destDirName)
        {
            return Api.bv_zip_get(pid, zipFileName, addFileName, destDirName);
        }

        public uint BvZipGetFileLength(string zipFileName, string fileName)
        {
            return Api.bv_zip_get_file_length(pid, zipFileName, fileName);
        }

        public uint BvZipReadBuffer(string zipFileName, string fileName, byte[] buffer, uint bufferLength)
        {
            return Api.bv_zip_read_buffer(pid, zipFileName, fileName, buffer, bufferLength);
        }

        public bool BvZipWriteBuffer(string zipFileName, string fileName, out byte[] buffer, uint bufferLength)
        {
            return Api.bv_zip_write_buffer(pid, zipFileName, fileName, out buffer, bufferLength);
        }

        public PLUGIN_ID pid;
    }

    #endregion
}

