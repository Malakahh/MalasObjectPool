The purpose of an object pool is essentially enabling recycling of objects, and it is done for performance reasons, see https://en.wikipedia.org/wiki/Object_pool_pattern.

This object pool features async instantiation of objects given a threshold, and will grow by N*2, ensuring a readily available supply of objects. Furthermore, it follows the Singleton pattern.