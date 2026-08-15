using System;
using System.Runtime.InteropServices;

namespace Evergine.Bindings.ThorVG
{
	public static unsafe partial class ThorVG
	{
		/// <summary>
		/// *********************************************************************
		/// Engine API
		/// *********************************************************************
		/// ThorVG requires an active runtime environment to operate.
		/// Internally, it utilizes a task scheduler to efficiently parallelize rendering operations.
		/// You can specify the number of worker threads using the
		/// parameter.
		/// During initialization, ThorVG will spawn the specified number of threads.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_engine_init", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_engine_init(uint threads);

		/// <summary>
		/// Cleans up resources and stops any internal threads initialized by tvg_engine_init().
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_engine_term", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_engine_term();

		[DllImport("thorvg", EntryPoint = "tvg_engine_version", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_engine_version(uint* major, uint* minor, uint* micro, byte** version);

		/// <summary>
		/// This method generates a software canvas instance that can be used for drawing vector graphics.
		/// It accepts an optional parameter
		/// to choose between different rendering engine behaviors.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_swcanvas_create", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr tvg_swcanvas_create(Tvg_Engine_Option op);

		/// <summary>
		/// For optimisation reasons TVG does not allocate memory for the output buffer on its own.
		/// The buffer of a desirable size should be allocated and owned by the caller.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_swcanvas_set_target", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_swcanvas_set_target(IntPtr canvas, uint* buffer, uint stride, uint w, uint h, Tvg_Colorspace cs);

		/// <summary>
		/// This method generates a OpenGL/ES canvas instance that can be used for drawing vector graphics.
		/// It accepts an optional parameter
		/// to choose between different rendering engine behaviors.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_glcanvas_create", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr tvg_glcanvas_create(Tvg_Engine_Option op);

		/// <summary>
		/// This function specifies the drawing target where the rasterization will occur. It can target
		/// a specific framebuffer object (FBO) or the main surface.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_glcanvas_set_target", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_glcanvas_set_target(IntPtr canvas, void* display, void* surface, void* context, int id, uint w, uint h, Tvg_Colorspace cs);

		/// <summary>
		/// This method generates a WebGPU canvas instance that can be used for drawing vector graphics.
		/// It accepts an optional parameter
		/// to choose between different rendering engine behaviors.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_wgcanvas_create", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr tvg_wgcanvas_create(Tvg_Engine_Option op);

		[DllImport("thorvg", EntryPoint = "tvg_wgcanvas_set_target", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_wgcanvas_set_target(IntPtr canvas, void* device, void* instance, void* target, uint w, uint h, Tvg_Colorspace cs, int type);

		[DllImport("thorvg", EntryPoint = "tvg_wgcanvas_set_target_with_context", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_wgcanvas_set_target_with_context(IntPtr canvas, Tvg_WgContext* context, void* target, uint w, uint h, Tvg_Colorspace cs, int type);

		/// <summary>
		/// *********************************************************************
		/// Common Canvas API
		/// *********************************************************************
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_canvas_destroy", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_canvas_destroy(IntPtr canvas);

		/// <summary>
		/// Adds the specified paint into the canvas root scene. Only paints added to
		/// the canvas are considered rendering targets. The canvas retains the paint
		/// object until it is explicitly removed via tvg_canvas_remove().
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_canvas_add", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_canvas_add(IntPtr canvas, IntPtr paint);

		/// <summary>
		/// Inserts a paint object into the root scene of the specified canvas. If the
		/// parameter is provided, the paint object is inserted immediately before
		/// the specified paint in the root scene. If
		/// is
		/// the paint object
		/// is appended to the end of the root scene.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_canvas_insert", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_canvas_insert(IntPtr canvas, IntPtr target, IntPtr at);

		/// <summary>
		/// This function removes a specified paint object from the root scene. If no paint
		/// object is specified (i.e., the default
		/// is used), the function
		/// performs to clear all paints from the scene.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_canvas_remove", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_canvas_remove(IntPtr canvas, IntPtr paint);

		/// <summary>
		/// This function triggers an internal update for all paint instances that have been modified
		/// since the last update. It ensures that the canvas state is ready for accurate rendering.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_canvas_update", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_canvas_update(IntPtr canvas);

		[DllImport("thorvg", EntryPoint = "tvg_canvas_draw", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_canvas_draw(IntPtr canvas, [MarshalAs(UnmanagedType.I1)] bool clear);

		/// <summary>
		/// The Canvas rendering can be performed asynchronously. To make sure that rendering is finished,
		/// the tvg_canvas_sync() must be called after the tvg_canvas_draw() regardless of threading.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_canvas_sync", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_canvas_sync(IntPtr canvas);

		/// <summary>
		/// This function defines a rectangular area of the canvas to be used for drawing operations.
		/// The specified viewport clips rendering output to the boundaries of that rectangle.
		/// Please note that changing the viewport is only allowed at the beginning of the rendering sequence—that is, after calling tvg_canvas_sync().
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_canvas_set_viewport", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_canvas_set_viewport(IntPtr canvas, int x, int y, int w, int h);

		/// <summary>
		/// This is the counterpart to the `new()` API, and releases the given Paint object safely,
		/// handling
		/// and managing ownership properly.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_paint_rel", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_paint_rel(IntPtr paint);

		/// <summary>
		/// This method increases the reference count of Tvg_Paint object, allowing shared ownership and control over its lifetime.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_paint_ref", CallingConvention = CallingConvention.Cdecl)]
		public static extern ushort tvg_paint_ref(IntPtr paint);

		/// <summary>
		/// This method decreases the reference count of the Tvg_Paint object by 1.
		/// If the reference count reaches zero and the
		/// flag is set to true, the instance is automatically deleted.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_paint_unref", CallingConvention = CallingConvention.Cdecl)]
		public static extern ushort tvg_paint_unref(IntPtr paint, [MarshalAs(UnmanagedType.I1)] bool free);

		/// <summary>
		/// This method provides the current reference count, allowing the user to check the shared ownership state of the Tvg_Paint object.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_paint_get_ref", CallingConvention = CallingConvention.Cdecl)]
		public static extern ushort tvg_paint_get_ref(IntPtr paint);

		/// <summary>
		/// This is useful for selectively excluding paint objects during rendering.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_paint_set_visible", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_paint_set_visible(IntPtr paint, [MarshalAs(UnmanagedType.I1)] bool visible);

		[DllImport("thorvg", EntryPoint = "tvg_paint_get_visible", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool tvg_paint_get_visible(IntPtr paint);

		[DllImport("thorvg", EntryPoint = "tvg_paint_get_id", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint tvg_paint_get_id(IntPtr paint);

		/// <summary>
		/// The ID is used to specify a paint instance in a scene.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_paint_set_id", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_paint_set_id(IntPtr paint, uint id);

		[DllImport("thorvg", EntryPoint = "tvg_paint_scale", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_paint_scale(IntPtr paint, float factor);

		/// <summary>
		/// The angle in measured clockwise from the horizontal axis.
		/// The rotational axis passes through the point on the object with zero coordinates.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_paint_rotate", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_paint_rotate(IntPtr paint, float degree);

		/// <summary>
		/// The origin of the coordinate system is in the upper-left corner of the canvas.
		/// The horizontal and vertical axes point to the right and down, respectively.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_paint_translate", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_paint_translate(IntPtr paint, float x, float y);

		/// <summary>
		/// The augmented matrix of the transformation is expected to be given.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_paint_set_transform", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_paint_set_transform(IntPtr paint, Tvg_Matrix* m);

		/// <summary>
		/// In case no transformation was applied, the identity matrix is returned.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_paint_get_transform", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_paint_get_transform(IntPtr paint, Tvg_Matrix* m);

		[DllImport("thorvg", EntryPoint = "tvg_paint_set_opacity", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_paint_set_opacity(IntPtr paint, byte opacity);

		[DllImport("thorvg", EntryPoint = "tvg_paint_get_opacity", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_paint_get_opacity(IntPtr paint, byte* opacity);

		/// <summary>
		/// Creates a new object and sets its all properties as in the original object.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_paint_duplicate", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr tvg_paint_duplicate(IntPtr paint);

		/// <summary>
		/// This function determines whether the specified rectangular region—defined by (`x`, `y`, `w`, `h`)—
		/// intersects the geometric fill region of the paint object.
		/// This is useful for hit-testing purposes, such as detecting whether a user interaction (e.g., touch or click)
		/// occurs within a painted region.
		/// The paint must be updated in a Canvas beforehand—typically after the Canvas has been
		/// drawn and synchronized.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_paint_intersects", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool tvg_paint_intersects(IntPtr paint, int x, int y, int w, int h);

		/// <summary>
		/// This function determines whether the specified rectangular region—defined by (`x`, `y`, `w`, `h`)—
		/// intersects the geometric fill region of the paint object.
		/// This is useful for hit-testing purposes, such as detecting whether a user interaction (e.g., touch or click)
		/// occurs within a painted region.
		/// The paint must be updated in a Canvas beforehand—typically after the Canvas has been
		/// drawn and synchronized.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_paint_intersects_region", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool tvg_paint_intersects_region(IntPtr paint, int x, int y, int w, int h, [MarshalAs(UnmanagedType.I1)] bool visibleOnly);

		/// <summary>
		/// Returns the bounding box of the paint as an axis-aligned bounding box (AABB), with all relevant transformations applied.
		/// The returned values
		/// may have invalid if the operation fails. Thus, please check the retval.
		/// This bounding box can be used to determine the actual rendered area of the object on the canvas,
		/// for purposes such as hit-testing, culling, or layout calculations.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_paint_get_aabb", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_paint_get_aabb(IntPtr paint, float* x, float* y, float* w, float* h);

		/// <summary>
		/// This function returns the bounding box of the paint, as an oriented bounding box (OBB) after transformations are applied.
		/// The returned values
		/// may have invalid if the operation fails. Thus, please check the retval.
		/// This bounding box can be used to obtain the transformed bounding region in canvas space
		/// by taking the geometry's axis-aligned bounding box (AABB) in the object's local coordinate space
		/// and applying the object's transformations.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_paint_get_obb", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_paint_get_obb(IntPtr paint, Tvg_Point* pt4);

		[DllImport("thorvg", EntryPoint = "tvg_paint_set_mask_method", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_paint_set_mask_method(IntPtr paint, IntPtr target, Tvg_Mask_Method method);

		[DllImport("thorvg", EntryPoint = "tvg_paint_get_mask_method", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_paint_get_mask_method(IntPtr paint, IntPtr target, Tvg_Mask_Method* method);

		/// <summary>
		/// This function restricts the drawing area of the paint object to the specified shape's paths.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_paint_set_clip", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_paint_set_clip(IntPtr paint, IntPtr clipper);

		/// <summary>
		/// This function returns the clipper that has been previously set to this paint object.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_paint_get_clip", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr tvg_paint_get_clip(IntPtr paint);

		/// <summary>
		/// This function returns the parent object if the current paint
		/// belongs to one. Otherwise, it returns
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_paint_get_parent", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr tvg_paint_get_parent(IntPtr paint);

		[DllImport("thorvg", EntryPoint = "tvg_paint_get_type", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_paint_get_type(IntPtr paint, Tvg_Type* type);

		/// <summary>
		/// The blending feature allows you to combine colors to create visually appealing effects, including transparency, lighting, shading, and color mixing, among others.
		/// its process involves the combination of colors or images from the source paint object with the destination (the lower layer image) using blending operations.
		/// The blending operation is determined by the chosen
		/// which specifies how the colors or images are combined.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_paint_set_blend_method", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_paint_set_blend_method(IntPtr paint, Tvg_Blend_Method method);

		/// <summary>
		/// This function allocates and returns a new Shape instance.
		/// To properly destroy the Shape object, use
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_shape_new", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr tvg_shape_new();

		/// <summary>
		/// The color, the fill and the stroke properties are retained.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_shape_reset", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_shape_reset(IntPtr paint);

		/// <summary>
		/// The value of the current point is set to the given point.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_shape_move_to", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_shape_move_to(IntPtr paint, float x, float y);

		/// <summary>
		/// The value of the current point is set to the given end-point.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_shape_line_to", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_shape_line_to(IntPtr paint, float x, float y);

		/// <summary>
		/// The Bezier curve starts at the current point and ends at the given end-point (
		/// Two control points (
		/// and (
		/// are used to determine the shape of the curve.
		/// The value of the current point is set to the given end-point.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_shape_cubic_to", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_shape_cubic_to(IntPtr paint, float cx1, float cy1, float cx2, float cy2, float x, float y);

		/// <summary>
		/// The value of the current point is set to the initial point of the closed sub-path.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_shape_close", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_shape_close(IntPtr paint);

		/// <summary>
		/// The rectangle with rounded corners can be achieved by setting non-zero values to
		/// and
		/// arguments.
		/// The
		/// and
		/// values specify the radii of the ellipse defining the rounding of the corners.
		/// The position of the rectangle is specified by the coordinates of its upper-left corner -
		/// and
		/// arguments.
		/// The rectangle is treated as a new sub-path - it is not connected with the previous sub-path.
		/// The value of the current point is set to (
		/// +
		/// +
		/// - in case
		/// is greater
		/// than
		/// the current point is set to (
		/// +
		/// +
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_shape_append_rect", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_shape_append_rect(IntPtr paint, float x, float y, float w, float h, float rx, float ry, [MarshalAs(UnmanagedType.I1)] bool cw);

		/// <summary>
		/// The position of the ellipse is specified by the coordinates of its center -
		/// and
		/// arguments.
		/// The ellipse is treated as a new sub-path - it is not connected with the previous sub-path.
		/// The value of the current point is set to (
		/// -
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_shape_append_circle", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_shape_append_circle(IntPtr paint, float cx, float cy, float rx, float ry, [MarshalAs(UnmanagedType.I1)] bool cw);

		/// <summary>
		/// The current point value is set to the last point from the sub-path.
		/// For each command from the
		/// array, an appropriate number of points in
		/// array should be specified.
		/// If the number of points in the
		/// array is different than the number required by the
		/// array, the shape with this sub-path will not be displayed on the screen.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_shape_append_path", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_shape_append_path(IntPtr paint, byte* cmds, uint cmdCnt, Tvg_Point* pts, uint ptsCnt);

		/// <summary>
		/// This function provides access to the shape's path data, including the commands
		/// and points that define the path.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_shape_get_path", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_shape_get_path(IntPtr paint, byte** cmds, uint* cmdsCnt, Tvg_Point** pts, uint* ptsCnt);

		/// <summary>
		/// This function defines the thickness of the stroke applied to all figures
		/// in the path object. A stroke is the outline drawn along the edges of the
		/// path's geometry.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_shape_set_stroke_width", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_shape_set_stroke_width(IntPtr paint, float width);

		[DllImport("thorvg", EntryPoint = "tvg_shape_get_stroke_width", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_shape_get_stroke_width(IntPtr paint, float* width);

		[DllImport("thorvg", EntryPoint = "tvg_shape_set_stroke_color", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_shape_set_stroke_color(IntPtr paint, byte r, byte g, byte b, byte a);

		[DllImport("thorvg", EntryPoint = "tvg_shape_get_stroke_color", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_shape_get_stroke_color(IntPtr paint, byte* r, byte* g, byte* b, byte* a);

		[DllImport("thorvg", EntryPoint = "tvg_shape_set_stroke_gradient", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_shape_set_stroke_gradient(IntPtr paint, IntPtr grad);

		/// <summary>
		/// The function does not allocate any memory.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_shape_get_stroke_gradient", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_shape_get_stroke_gradient(IntPtr paint, IntPtr* grad);

		[DllImport("thorvg", EntryPoint = "tvg_shape_set_stroke_dash", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_shape_set_stroke_dash(IntPtr paint, float* dashPattern, uint cnt, float offset);

		/// <summary>
		/// The function does not allocate any memory.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_shape_get_stroke_dash", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_shape_get_stroke_dash(IntPtr paint, float** dashPattern, uint* cnt, float* offset);

		/// <summary>
		/// The cap style specifies the shape to be used at the end of the open stroked sub-paths.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_shape_set_stroke_cap", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_shape_set_stroke_cap(IntPtr paint, Tvg_Stroke_Cap cap);

		[DllImport("thorvg", EntryPoint = "tvg_shape_get_stroke_cap", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_shape_get_stroke_cap(IntPtr paint, Tvg_Stroke_Cap* cap);

		[DllImport("thorvg", EntryPoint = "tvg_shape_set_stroke_join", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_shape_set_stroke_join(IntPtr paint, Tvg_Stroke_Join join);

		[DllImport("thorvg", EntryPoint = "tvg_shape_get_stroke_join", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_shape_get_stroke_join(IntPtr paint, Tvg_Stroke_Join* join);

		[DllImport("thorvg", EntryPoint = "tvg_shape_set_stroke_miterlimit", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_shape_set_stroke_miterlimit(IntPtr paint, float miterlimit);

		[DllImport("thorvg", EntryPoint = "tvg_shape_get_stroke_miterlimit", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_shape_get_stroke_miterlimit(IntPtr paint, float* miterlimit);

		/// <summary>
		/// If the values of the arguments
		/// and
		/// exceed the 0-1 range, they are wrapped around in a manner similar to angle wrapping, effectively treating the range as circular.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_shape_set_trimpath", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_shape_set_trimpath(IntPtr paint, float begin, float end, [MarshalAs(UnmanagedType.I1)] bool simultaneous);

		/// <summary>
		/// The parts of the shape defined as inner are colored.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_shape_set_fill_color", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_shape_set_fill_color(IntPtr paint, byte r, byte g, byte b, byte a);

		[DllImport("thorvg", EntryPoint = "tvg_shape_get_fill_color", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_shape_get_fill_color(IntPtr paint, byte* r, byte* g, byte* b, byte* a);

		/// <summary>
		/// Specifies how the interior of the shape is determined when its path intersects itself.
		/// The default fill rule is
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_shape_set_fill_rule", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_shape_set_fill_rule(IntPtr paint, Tvg_Fill_Rule rule);

		/// <summary>
		/// This function returns the fill rule, which determines how the interior
		/// regions of the shape are calculated when it overlaps itself.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_shape_get_fill_rule", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_shape_get_fill_rule(IntPtr paint, Tvg_Fill_Rule* rule);

		[DllImport("thorvg", EntryPoint = "tvg_shape_set_paint_order", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_shape_set_paint_order(IntPtr paint, [MarshalAs(UnmanagedType.I1)] bool strokeFirst);

		/// <summary>
		/// The parts of the shape defined as inner are filled.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_shape_set_gradient", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_shape_set_gradient(IntPtr paint, IntPtr grad);

		/// <summary>
		/// The function does not allocate any data.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_shape_get_gradient", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_shape_get_gradient(IntPtr paint, IntPtr* grad);

		/// <summary>
		/// *********************************************************************
		/// Gradient API
		/// *********************************************************************
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_linear_gradient_new", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr tvg_linear_gradient_new();

		[DllImport("thorvg", EntryPoint = "tvg_radial_gradient_new", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr tvg_radial_gradient_new();

		/// <summary>
		/// The bounds of the linear gradient are defined as a surface constrained by two parallel lines crossing
		/// the given points (
		/// and (
		/// respectively. Both lines are perpendicular to the line linking
		/// (
		/// and (
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_linear_gradient_set", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_linear_gradient_set(IntPtr grad, float x1, float y1, float x2, float y2);

		/// <summary>
		/// The bounds of the linear gradient are defined as a surface constrained by two parallel lines crossing
		/// the given points (
		/// and (
		/// respectively. Both lines are perpendicular to the line linking
		/// (
		/// and (
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_linear_gradient_get", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_linear_gradient_get(IntPtr grad, float* x1, float* y1, float* x2, float* y2);

		/// <summary>
		/// The radial gradient is defined by the end circle with a center (
		/// and a radius
		/// and
		/// the start circle with a center/focal point (
		/// and a radius
		/// The gradient will be rendered such that the gradient stop at an offset of 100% aligns with the edge of the end circle
		/// and the stop at an offset of 0% aligns with the edge of the start circle.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_radial_gradient_set", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_radial_gradient_set(IntPtr grad, float cx, float cy, float r, float fx, float fy, float fr);

		[DllImport("thorvg", EntryPoint = "tvg_radial_gradient_get", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_radial_gradient_get(IntPtr grad, float* cx, float* cy, float* r, float* fx, float* fy, float* fr);

		[DllImport("thorvg", EntryPoint = "tvg_gradient_set_color_stops", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_gradient_set_color_stops(IntPtr grad, Tvg_Color_Stop* color_stop, uint cnt);

		/// <summary>
		/// The function does not allocate any memory.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_gradient_get_color_stops", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_gradient_get_color_stops(IntPtr grad, Tvg_Color_Stop** color_stop, uint* cnt);

		[DllImport("thorvg", EntryPoint = "tvg_gradient_set_spread", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_gradient_set_spread(IntPtr grad, Tvg_Stroke_Fill spread);

		[DllImport("thorvg", EntryPoint = "tvg_gradient_get_spread", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_gradient_get_spread(IntPtr grad, Tvg_Stroke_Fill* spread);

		/// <summary>
		/// The augmented matrix of the transformation is expected to be given.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_gradient_set_transform", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_gradient_set_transform(IntPtr grad, Tvg_Matrix* m);

		/// <summary>
		/// In case no transformation was applied, the identity matrix is set.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_gradient_get_transform", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_gradient_get_transform(IntPtr grad, Tvg_Matrix* m);

		[DllImport("thorvg", EntryPoint = "tvg_gradient_get_type", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_gradient_get_type(IntPtr grad, Tvg_Type* type);

		/// <summary>
		/// Creates a new object and sets its all properties as in the original object.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_gradient_duplicate", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr tvg_gradient_duplicate(IntPtr grad);

		[DllImport("thorvg", EntryPoint = "tvg_gradient_del", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_gradient_del(IntPtr grad);

		/// <summary>
		/// This function allocates and returns a new Picture instance.
		/// To properly destroy the Picture object, use
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_picture_new", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr tvg_picture_new();

		/// <summary>
		/// ThorVG efficiently caches the loaded data using the specified
		/// as a key.
		/// This means that loading the same file again will not result in duplicate operations;
		/// instead, ThorVG will reuse the previously loaded picture data.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_picture_load", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_picture_load(IntPtr picture, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);

		/// <summary>
		/// ThorVG efficiently caches the loaded data, using the provided
		/// address as a key
		/// when
		/// is set to
		/// This allows ThorVG to avoid redundant operations
		/// by reusing the previously loaded picture data for the same sharable
		/// rather than duplicating the load process.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_picture_load_raw", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_picture_load_raw(IntPtr picture, uint* data, uint w, uint h, Tvg_Colorspace cs, [MarshalAs(UnmanagedType.I1)] bool copy);

		/// <summary>
		/// ThorVG efficiently caches the loaded data using the specified
		/// address as a key
		/// when the
		/// has
		/// This means that loading the same data again will not result in duplicate operations
		/// for the sharable
		/// Instead, ThorVG will reuse the previously loaded picture data.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_picture_load_data", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_picture_load_data(IntPtr picture, byte* data, uint size, [MarshalAs(UnmanagedType.LPUTF8Str)] string mimetype, [MarshalAs(UnmanagedType.LPUTF8Str)] string rpath, [MarshalAs(UnmanagedType.I1)] bool copy);

		/// <summary>
		/// This callback is invoked when an external asset reference (such as an image source or file path)
		/// is encountered in a Picture object. It allows the user to provide a custom mechanism for loading
		/// or substituting assets, such as loading from an external source or a virtual filesystem.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_picture_set_asset_resolver", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_picture_set_asset_resolver(IntPtr picture, delegate* unmanaged[Cdecl]<IntPtr, byte*, void*, bool> resolver, void* data);

		/// <summary>
		/// The picture content is resized while keeping the default size aspect ratio.
		/// The scaling factor is established for each of dimensions and the smaller value is applied to both of them.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_picture_set_size", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_picture_set_size(IntPtr picture, float w, float h);

		[DllImport("thorvg", EntryPoint = "tvg_picture_get_size", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_picture_get_size(IntPtr picture, float* w, float* h);

		/// <summary>
		/// This method defines the origin point of the Picture using normalized coordinates.
		/// Unlike a typical pivot point used only for transformations, this origin affects both
		/// the transformation behavior and the actual rendering position of the Picture.
		/// The specified origin becomes the reference point for positioning the Picture on the canvas.
		/// For example, setting the origin to (0.5f, 0.5f) moves the visual center of the picture
		/// to the position specified by Paint::translate().
		/// The coordinates are given in a normalized range relative to the picture's bounds:
		/// - (0.0f, 0.0f): top-left corner
		/// - (0.5f, 0.5f): center
		/// - (1.0f, 1.0f): bottom-right corner
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_picture_set_origin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_picture_set_origin(IntPtr picture, float x, float y);

		/// <summary>
		/// This method retrieves the current origin point of the Picture, expressed
		/// in normalized coordinates relative to the picture’s bounds.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_picture_get_origin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_picture_get_origin(IntPtr picture, float* x, float* y);

		/// <summary>
		/// This function searches for a paint object within the Picture scene that matches the provided
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_picture_get_paint", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr tvg_picture_get_paint(IntPtr picture, uint id);

		/// <summary>
		/// Specifies how the image data should be filtered when it is scaled or transformed
		/// during rendering. This affects the visual quality and performance of the output.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_picture_set_filter", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_picture_set_filter(IntPtr picture, Tvg_Filter_Method method);

		/// <summary>
		/// When accessible mode is enabled, the Picture maintains an internal mapping
		/// of ID-accessible vector assets nodes (such as SVG), allowing efficient access to Paint objects
		/// and their associated identifier information via Accessor APIs.
		/// When disabled, no additional mapping is maintained and all nodes are treated
		/// as general traversal targets.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_picture_set_accessible", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_picture_set_accessible(IntPtr picture, [MarshalAs(UnmanagedType.I1)] bool accessible);

		/// <summary>
		/// This function allocates and returns a new Scene instance.
		/// To properly destroy the Scene object, use
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_scene_new", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr tvg_scene_new();

		/// <summary>
		/// Appends the specified paint object to the given scene. Only paint objects
		/// added to the scene are considered rendering targets.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_scene_add", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_scene_add(IntPtr scene, IntPtr paint);

		/// <summary>
		/// Inserts the specified paint object into the scene immediately before the
		/// given paint object
		/// The
		/// parameter must reference an existing
		/// paint object already added to the scene.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_scene_insert", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_scene_insert(IntPtr scene, IntPtr target, IntPtr at);

		/// <summary>
		/// This function removes a specified paint object from the scene. If no paint
		/// object is specified (i.e., the default
		/// is used), the function
		/// performs to clear all paints from the scene.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_scene_remove", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_scene_remove(IntPtr scene, IntPtr paint);

		/// <summary>
		/// This function clears all effects that have been applied to the scene,
		/// restoring it to its original state without any post-processing.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_scene_clear_effects", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_scene_clear_effects(IntPtr scene);

		/// <summary>
		/// This function adds a Gaussian blur filter to the scene as a post-processing effect.
		/// The blur can be applied in different directions with configurable border handling and quality settings.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_scene_add_effect_gaussian_blur", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_scene_add_effect_gaussian_blur(IntPtr scene, double sigma, int direction, int border, int quality);

		/// <summary>
		/// This function adds a drop shadow with a Gaussian blur to the scene. The shadow
		/// can be customized using color, opacity, angle, distance, blur radius (sigma),
		/// and quality parameters.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_scene_add_effect_drop_shadow", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_scene_add_effect_drop_shadow(IntPtr scene, int r, int g, int b, int a, double angle, double distance, double sigma, int quality);

		/// <summary>
		/// This function overrides the scene's content colors with the specified fill color.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_scene_add_effect_fill", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_scene_add_effect_fill(IntPtr scene, int r, int g, int b, int a);

		/// <summary>
		/// This function tints the current scene using specified black and white color values,
		/// modulated by a given intensity.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_scene_add_effect_tint", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_scene_add_effect_tint(IntPtr scene, int black_r, int black_g, int black_b, int white_r, int white_g, int white_b, double intensity);

		/// <summary>
		/// This function adds a tritone color effect to the given scene using three sets of RGB values
		/// representing shadow, midtone, and highlight colors.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_scene_add_effect_tritone", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_scene_add_effect_tritone(IntPtr scene, int shadow_r, int shadow_g, int shadow_b, int midtone_r, int midtone_g, int midtone_b, int highlight_r, int highlight_g, int highlight_b, int blend);

		/// <summary>
		/// This function allocates and returns a new Text instance.
		/// To properly destroy the Text object, use
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_text_new", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr tvg_text_new();

		/// <summary>
		/// This function specifies the name of the font to be used when rendering text.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_text_set_font", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_text_set_font(IntPtr text, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);

		/// <summary>
		/// This function sets the font size used during text rendering.
		/// The size is specified in point units, and supports floating-point precision
		/// for smooth scaling and animation effects.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_text_set_size", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_text_set_size(IntPtr text, float size);

		/// <summary>
		/// This function sets the unicode text that will be displayed by the rendering system.
		/// The text is set according to the specified UTF encoding method, which defaults to UTF-8.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_text_set_text", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_text_set_text(IntPtr text, [MarshalAs(UnmanagedType.LPUTF8Str)] string utf8);

		/// <summary>
		/// This function retrieves the unicode string that is currently set
		/// for rendering. The returned text is encoded in UTF-8.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_text_get_text", CallingConvention = CallingConvention.Cdecl)]
		public static extern byte* tvg_text_get_text(IntPtr text);

		/// <summary>
		/// If layout width/height is set on an axis, align within the layout box.
		/// Otherwise, treat it as an anchor within the text bounds which point of
		/// the text box is pinned to the paint position.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_text_align", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_text_align(IntPtr text, float x, float y);

		/// <summary>
		/// If width/height is set on an axis, that axis is constrained by a virtual layout box and
		/// the text may wrap/align inside it. If width/height == 0, the axis is
		/// unconstrained and
		/// acts as an anchor on that axis.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_text_layout", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_text_layout(IntPtr text, float w, float h);

		/// <summary>
		/// This method controls how the text is laid out when it exceeds the available space.
		/// The wrapping mode determines whether text is truncated, wrapped by character or word,
		/// or adjusted automatically. An ellipsis mode is also available for truncation with "...".
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_text_wrap_mode", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_text_wrap_mode(IntPtr text, Tvg_Text_Wrap mode);

		/// <summary>
		/// This function retrieves the number of lines generated after applying text layout and wrapping.
		/// The returned value reflects the current wrapping configuration set by tvg_text_wrap_mode().
		/// The line count is also increased by explicit line feed characters ('
		/// ') contained in the text.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_text_line_count", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint tvg_text_line_count(IntPtr text);

		/// <summary>
		/// This function adjusts the letter spacing (horizontal space between glyphs) and
		/// line spacing (vertical space between lines of text) using scale factors.
		/// Both values are relative to the font's default metrics:
		/// - The letter spacing is applied as a scale factor to the glyph's advance width.
		/// - The line spacing is applied as a scale factor to the glyph's advance height.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_text_spacing", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_text_spacing(IntPtr text, float letter, float line);

		/// <summary>
		/// This function applies a shear transformation to simulate an italic (oblique) style
		/// for the current text object. The shear factor determines the degree of slant
		/// applied along the X-axis.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_text_set_italic", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_text_set_italic(IntPtr text, float shear);

		/// <summary>
		/// This function adds an outline to the text with the specified width and RGB color.
		/// The outline enhances the visibility of the text by rendering a stroke around its glyphs.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_text_set_outline", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_text_set_outline(IntPtr text, float width, byte r, byte g, byte b);

		[DllImport("thorvg", EntryPoint = "tvg_text_set_color", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_text_set_color(IntPtr text, byte r, byte g, byte b);

		[DllImport("thorvg", EntryPoint = "tvg_text_set_gradient", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_text_set_gradient(IntPtr text, IntPtr gradient);

		/// <summary>
		/// Fills the provided
		/// structure with the font layout values of this text object,
		/// such as ascent, descent, linegap, and line advance.
		/// The returned values reflect the font size applied to the text object,
		/// but do not include any transformations (e.g., scale, rotation, or translation).
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_text_get_text_metrics", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_text_get_text_metrics(IntPtr text, Tvg_Text_Metrics* metrics);

		/// <summary>
		/// Fills the provided
		/// structure with the horizontal layout values
		/// of the specified glyph, such as advance, left-side bearing, and bounding box.
		/// The returned values reflect the font size applied to the text object,
		/// but do not include any transformations (e.g., scale, rotation, or translation).
		/// The input character must be a single UTF-8 encoded character.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_text_get_glyph_metrics", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_text_get_glyph_metrics(IntPtr text, [MarshalAs(UnmanagedType.LPUTF8Str)] string ch, Tvg_Glyph_Metrics* metrics, byte** next);

		/// <summary>
		/// ThorVG efficiently caches the loaded data using the specified
		/// as a key.
		/// This means that loading the same file again will not result in duplicate operations;
		/// instead, ThorVG will reuse the previously loaded font data.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_font_load", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_font_load([MarshalAs(UnmanagedType.LPUTF8Str)] string path);

		/// <summary>
		/// ThorVG efficiently caches the loaded font data using the specified
		/// as a key.
		/// This means that loading the same fonts again will not result in duplicate operations.
		/// Instead, ThorVG will reuse the previously loaded font data.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_font_load_data", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_font_load_data([MarshalAs(UnmanagedType.LPUTF8Str)] string name, byte* data, uint size, [MarshalAs(UnmanagedType.LPUTF8Str)] string mimetype, [MarshalAs(UnmanagedType.I1)] bool copy);

		/// <summary>
		/// This function is used to release resources associated with a font file that has been loaded into memory.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_font_unload", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_font_unload([MarshalAs(UnmanagedType.LPUTF8Str)] string path);

		/// <summary>
		/// *********************************************************************
		/// Saver API
		/// *********************************************************************
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_saver_new", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr tvg_saver_new();

		/// <summary>
		/// If the saver module supports any compression mechanism, it will optimize the data size.
		/// This might affect the encoding/decoding time in some cases. You can turn off the compression
		/// if you wish to optimize for speed.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_saver_save_paint", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_saver_save_paint(IntPtr saver, IntPtr paint, [MarshalAs(UnmanagedType.LPUTF8Str)] string path, uint quality);

		/// <summary>
		/// If the saver module supports any compression mechanism, it will optimize the data size.
		/// This might affect the encoding/decoding time in some cases. You can turn off the compression
		/// if you wish to optimize for speed.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_saver_save_animation", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_saver_save_animation(IntPtr saver, IntPtr animation, [MarshalAs(UnmanagedType.LPUTF8Str)] string path, uint quality, uint fps);

		/// <summary>
		/// The behavior of the Saver module works on a sync/async basis, depending on the threading setting of the Initializer.
		/// Thus, if you wish to have a benefit of it, you must call tvg_saver_sync() after the tvg_saver_save_paint() in the proper delayed time.
		/// Otherwise, you can call tvg_saver_sync() immediately.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_saver_sync", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_saver_sync(IntPtr saver);

		[DllImport("thorvg", EntryPoint = "tvg_saver_del", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_saver_del(IntPtr saver);

		[DllImport("thorvg", EntryPoint = "tvg_animation_new", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr tvg_animation_new();

		[DllImport("thorvg", EntryPoint = "tvg_animation_set_frame", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_animation_set_frame(IntPtr animation, float no);

		/// <summary>
		/// This function provides access to the picture instance that can be used to load animation formats, such as lot.
		/// After setting up the picture, it can be added to the designated canvas, enabling control over animation frames
		/// with this Animation instance.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_animation_get_picture", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr tvg_animation_get_picture(IntPtr animation);

		[DllImport("thorvg", EntryPoint = "tvg_animation_get_frame", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_animation_get_frame(IntPtr animation, float* no);

		[DllImport("thorvg", EntryPoint = "tvg_animation_get_total_frame", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_animation_get_total_frame(IntPtr animation, float* cnt);

		[DllImport("thorvg", EntryPoint = "tvg_animation_get_duration", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_animation_get_duration(IntPtr animation, float* duration);

		/// <summary>
		/// The set segment is designated as the play area of the animation.
		/// This is useful for playing a specific segment within the entire animation.
		/// After setting, the number of animation frames and the playback time are calculated
		/// by mapping the playback segment as the entire range.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_animation_set_segment", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_animation_set_segment(IntPtr animation, float begin, float end);

		[DllImport("thorvg", EntryPoint = "tvg_animation_get_segment", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_animation_get_segment(IntPtr animation, float* begin, float* end);

		[DllImport("thorvg", EntryPoint = "tvg_animation_del", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_animation_del(IntPtr animation);

		/// <summary>
		/// *********************************************************************
		/// Accessor API
		/// *********************************************************************
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_accessor_new", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr tvg_accessor_new();

		[DllImport("thorvg", EntryPoint = "tvg_accessor_del", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_accessor_del(IntPtr accessor);

		/// <summary>
		/// Iterates through all descendents of the scene passed through the paint argument
		/// while calling func on each and passing the data pointer to this function. When
		/// func returns false iteration stops and the function returns.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_accessor_set", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_accessor_set(IntPtr accessor, IntPtr paint, delegate* unmanaged[Cdecl]<IntPtr, void*, bool> func, void* data);

		/// <summary>
		/// This function computes a unique identifier value based on the provided string.
		/// You can use this to assign a unique ID to the Paint object.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_accessor_generate_id", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint tvg_accessor_generate_id([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

		/// <summary>
		/// Returns the name associated with the specified identifier.
		/// This method is only valid when
		/// is set to
		/// for the Picture associated with the given
		/// in
		/// Otherwise, the name
		/// information may not be available.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_accessor_get_name", CallingConvention = CallingConvention.Cdecl)]
		public static extern byte* tvg_accessor_get_name(IntPtr accessor, uint id);

		[DllImport("thorvg", EntryPoint = "tvg_lottie_animation_new", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr tvg_lottie_animation_new();

		[DllImport("thorvg", EntryPoint = "tvg_lottie_animation_gen_slot", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint tvg_lottie_animation_gen_slot(IntPtr animation, [MarshalAs(UnmanagedType.LPUTF8Str)] string slot);

		[DllImport("thorvg", EntryPoint = "tvg_lottie_animation_apply_slot", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_lottie_animation_apply_slot(IntPtr animation, uint id);

		[DllImport("thorvg", EntryPoint = "tvg_lottie_animation_del_slot", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_lottie_animation_del_slot(IntPtr animation, uint id);

		[DllImport("thorvg", EntryPoint = "tvg_lottie_animation_set_marker", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_lottie_animation_set_marker(IntPtr animation, [MarshalAs(UnmanagedType.LPUTF8Str)] string marker);

		[DllImport("thorvg", EntryPoint = "tvg_lottie_animation_get_markers_cnt", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_lottie_animation_get_markers_cnt(IntPtr animation, uint* cnt);

		[DllImport("thorvg", EntryPoint = "tvg_lottie_animation_get_marker", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_lottie_animation_get_marker(IntPtr animation, uint idx, byte** name);

		[DllImport("thorvg", EntryPoint = "tvg_lottie_animation_get_marker_info", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_lottie_animation_get_marker_info(IntPtr animation, uint idx, byte** name, float* begin, float* end);

		/// <summary>
		/// This method performs tweening, a process of generating intermediate frame
		/// between
		/// and
		/// based on the given
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_lottie_animation_tween", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_lottie_animation_tween(IntPtr animation, float from, float to, float progress);

		/// <summary>
		/// This method starts a dynamic interpolation from the current animation frame
		/// toward
		/// Use tvg_lottie_animation_tween_go() to update the interpolation progress.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_lottie_animation_tween_to", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_lottie_animation_tween_to(IntPtr animation, float to);

		/// <summary>
		/// This method advances the interpolation started by
		/// using the
		/// given
		/// value.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_lottie_animation_tween_go", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_lottie_animation_tween_go(IntPtr animation, float progress);

		/// <summary>
		/// This function controls the rendering quality of effects like blur, shadows, etc.
		/// Lower values prioritize performance while higher values prioritize quality.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_lottie_animation_set_quality", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_lottie_animation_set_quality(IntPtr animation, byte value);

		/// <summary>
		/// The resolver is invoked whenever the playback state of an audio layer changes.
		/// It allows applications to synchronize audio playback with the animation timeline.
		/// </summary>
		[DllImport("thorvg", EntryPoint = "tvg_lottie_animation_set_audio_resolver", CallingConvention = CallingConvention.Cdecl)]
		public static extern Tvg_Result tvg_lottie_animation_set_audio_resolver(IntPtr animation, delegate* unmanaged[Cdecl]<Tvg_Audio_Info*, void*, void> resolver, void* data);
	}
}
