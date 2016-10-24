The purpose of an object pool is essentially enabling recycling of objects, and it is done for performance reasons, see Wikipedia.<b><b><b><b>

This object pool features async instantiation of objects given a threshold, and will grow by N*2, ensuring a readily available supply of objects. Furthermore, it follows the Singleton pattern.